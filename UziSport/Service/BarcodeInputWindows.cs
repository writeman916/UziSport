#if WINDOWS
using System.Runtime.InteropServices;
using System.Text;

namespace UziSport.Services;

public sealed class BarcodeInputWindows : IBarcodeInput
{
    public event EventHandler<string>? BarcodeScanned;

    private nint _hwnd;
    private nint _oldWndProc;
    private WndProcDelegate? _newWndProc;

    private readonly StringBuilder _buffer = new();
    private long _lastTick;
    private const int MaxGapMs = 60;   // gap giữa các ký tự barcode thường rất nhỏ
    private const int MinLen = 4;

    private bool _scanMode = true;

    public void Start(nint hwnd)
    {
        if (hwnd == 0) return;
        if (_hwnd != 0) return;

        _hwnd = hwnd;

        // nhận input keyboard dù không focus
        var rid = new RAWINPUTDEVICE
        {
            usUsagePage = 0x01, // Generic Desktop Controls
            usUsage = 0x06,     // Keyboard
            dwFlags = RIDEV_INPUTSINK | (_scanMode ? RIDEV_NOLEGACY : 0),
            hwndTarget = _hwnd
        };

        if (!RegisterRawInputDevices(new[] { rid }, 1, (uint)Marshal.SizeOf<RAWINPUTDEVICE>()))
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "RegisterRawInputDevices failed");

        _newWndProc = WndProc;
        _oldWndProc = SetWindowLongPtr(_hwnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_newWndProc));
    }

    public void Stop()
    {
        if (_hwnd == 0) return;

        // ✅ trả lại legacy key trước khi rời trang
        try { SetScanMode(false); } catch { }

        if (_oldWndProc != 0)
            SetWindowLongPtr(_hwnd, GWLP_WNDPROC, _oldWndProc);

        _hwnd = 0;
        _oldWndProc = 0;
        _newWndProc = null;

        _buffer.Clear();
        _lastTick = 0;
    }

    public void SetScanMode(bool enabled)
    {
        _scanMode = enabled;

        if (_hwnd == 0) return;

        var rid = new RAWINPUTDEVICE
        {
            usUsagePage = 0x01,
            usUsage = 0x06,
            dwFlags = RIDEV_INPUTSINK | (_scanMode ? RIDEV_NOLEGACY : 0),
            hwndTarget = _hwnd
        };

        RegisterRawInputDevices(new[] { rid }, 1, (uint)Marshal.SizeOf<RAWINPUTDEVICE>());
    }

    private nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        if (msg == WM_INPUT)
        {
            try { HandleRawInput(lParam); } catch { /* tránh crash UI */ }
        }

        return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
    }

    private void HandleRawInput(nint lParam)
    {
        // chỉ xử lý khi app đang foreground (để khỏi “ăn” scan khi bạn đang dùng app khác)
        if (GetForegroundWindow() != _hwnd) return;

        uint dwSize = 0;
        GetRawInputData(lParam, RID_INPUT, nint.Zero, ref dwSize, (uint)Marshal.SizeOf<RAWINPUTHEADER>());
        if (dwSize == 0) return;

        var pData = Marshal.AllocHGlobal((int)dwSize);
        try
        {
            if (GetRawInputData(lParam, RID_INPUT, pData, ref dwSize, (uint)Marshal.SizeOf<RAWINPUTHEADER>()) != dwSize)
                return;

            var header = Marshal.PtrToStructure<RAWINPUTHEADER>(pData);
            if (header.dwType != RIM_TYPEKEYBOARD) return;

            int headerSize = Marshal.SizeOf<RAWINPUTHEADER>();
            var kbd = Marshal.PtrToStructure<RAWKEYBOARD>(nint.Add(pData, headerSize));

            // chỉ lấy KeyDown
            if (kbd.Message != WM_KEYDOWN && kbd.Message != WM_SYSKEYDOWN) return;

            var now = Environment.TickCount64;
            var gap = _lastTick == 0 ? 0 : (int)(now - _lastTick);
            _lastTick = now;

            // nếu gõ chậm -> coi như không phải barcode, reset buffer
            if (_buffer.Length > 0 && gap > MaxGapMs)
                _buffer.Clear();

            ushort vkey = kbd.VKey;

            // Enter/Tab kết thúc barcode (khuyến nghị cấu hình scanner gửi Enter)
            if (vkey == VK_RETURN || vkey == VK_TAB)
            {
                if (_buffer.Length >= MinLen)
                {
                    var code = _buffer.ToString();
                    _buffer.Clear();
                    BarcodeScanned?.Invoke(this, code);
                }
                else
                {
                    _buffer.Clear();
                }
                return;
            }

            // bỏ qua phím điều khiển
            if (vkey is VK_SHIFT or VK_CONTROL or VK_MENU or VK_LWIN or VK_RWIN)
                return;

            var ch = VkToChar(vkey, kbd.MakeCode, kbd.Flags);
            if (ch != '\0')
                _buffer.Append(ch);
        }
        finally
        {
            Marshal.FreeHGlobal(pData);
        }
    }

    private static char VkToChar(uint vkey, ushort makeCode, ushort flags)
    {
        // translate vkey -> unicode
        byte[] state = new byte[256];
        if (!GetKeyboardState(state)) return '\0';

        var hkl = GetKeyboardLayout(0);

        uint scanCode = makeCode;
        if ((flags & RI_KEY_E0) != 0)
            scanCode |= 0xE000;

        var sb = new StringBuilder(8);
        int rc = ToUnicodeEx(vkey, scanCode, state, sb, sb.Capacity, 0, hkl);
        if (rc == 1) return sb[0];
        return '\0';
    }

    // ================= Win32 =================

    private delegate nint WndProcDelegate(nint hWnd, uint msg, nint wParam, nint lParam);

    private const uint WM_INPUT = 0x00FF;
    private const uint WM_KEYDOWN = 0x0100;
    private const uint WM_SYSKEYDOWN = 0x0104;

    private const int GWLP_WNDPROC = -4;

    private const uint RID_INPUT = 0x10000003;
    private const uint RIM_TYPEKEYBOARD = 1;

    private const uint RIDEV_INPUTSINK = 0x00000100;
    private const uint RIDEV_NOLEGACY = 0x00000030;

    private const ushort RI_KEY_E0 = 0x02;

    private const ushort VK_RETURN = 0x0D;
    private const ushort VK_TAB = 0x09;
    private const ushort VK_SHIFT = 0x10;
    private const ushort VK_CONTROL = 0x11;
    private const ushort VK_MENU = 0x12; // Alt
    private const ushort VK_LWIN = 0x5B;
    private const ushort VK_RWIN = 0x5C;

    [StructLayout(LayoutKind.Sequential)]
    private struct RAWINPUTDEVICE
    {
        public ushort usUsagePage;
        public ushort usUsage;
        public uint dwFlags;
        public nint hwndTarget;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RAWINPUTHEADER
    {
        public uint dwType;
        public uint dwSize;
        public nint hDevice;
        public nint wParam;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RAWKEYBOARD
    {
        public ushort MakeCode;
        public ushort Flags;
        public ushort Reserved;
        public ushort VKey;
        public uint Message;
        public uint ExtraInformation;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterRawInputDevices(RAWINPUTDEVICE[] pRawInputDevices, uint uiNumDevices, uint cbSize);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetRawInputData(nint hRawInput, uint uiCommand, nint pData, ref uint pcbSize, uint cbSizeHeader);

    [DllImport("user32.dll")]
    private static extern bool GetKeyboardState(byte[] lpKeyState);

    [DllImport("user32.dll")]
    private static extern nint GetKeyboardLayout(uint idThread);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState,
        [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags, nint dwhkl);

    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();

    [DllImport("user32.dll", EntryPoint = "CallWindowProcW")]
    private static extern nint CallWindowProc(nint lpPrevWndFunc, nint hWnd, uint Msg, nint wParam, nint lParam);

    private static nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong)
        => IntPtr.Size == 8 ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong) : SetWindowLong32(hWnd, nIndex, dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern nint SetWindowLongPtr64(nint hWnd, int nIndex, nint dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
    private static extern nint SetWindowLong32(nint hWnd, int nIndex, nint dwNewLong);
}
#endif
