namespace UziSport;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();

        // Luôn dùng giao diện sáng
        Current.UserAppTheme = AppTheme.Light;
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);

#if WINDOWS
        window.Created += (s, e) =>
        {
            // Lấy native window (Microsoft.UI.Xaml.Window)
            var nativeWindow = window.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
            if (nativeWindow is null)
                return;

            // Lấy AppWindow từ handle
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            if (appWindow is null)
                return;

            // KÍCH THƯỚC CỐ ĐỊNH
            int width = 1550;
            int height = 1000;
            appWindow.Resize(new Windows.Graphics.SizeInt32(width, height));

            // TẮT PHÓNG TO + TẮT RESIZE
            if (appWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
            {
                presenter.IsResizable = false;     // không cho kéo giãn
                presenter.IsMaximizable = false;   // không cho bấm maximize
                // Nếu muốn tắt luôn minimize:
                // presenter.IsMinimizable = false;
            }
        };
#endif

        return window;
    }
}
