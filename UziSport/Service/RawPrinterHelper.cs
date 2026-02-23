using System;
using System.Runtime.InteropServices;
using UziSport.Model;

namespace UziSport.Service
{
    public static class RawPrinterHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private class DOCINFOW
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string pDocName;
            [MarshalAs(UnmanagedType.LPWStr)] public string pOutputFile;
            [MarshalAs(UnmanagedType.LPWStr)] public string pDataType;
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterW", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        [DllImport("winspool.Drv", SetLastError = true)]
        private static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterW", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In] DOCINFOW di);

        [DllImport("winspool.Drv", SetLastError = true)]
        private static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        private static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        private static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        public static void SendBytes(string printerName, byte[] bytes, string docName = "Receipt")
        {
            if (bytes == null || bytes.Length == 0) return;

            if (!OpenPrinter(printerName, out var hPrinter, IntPtr.Zero))
                throw new InvalidOperationException("Không mở được printer: " + printerName);

            try
            {
                var di = new DOCINFOW { pDocName = docName, pDataType = "RAW" };
                if (!StartDocPrinter(hPrinter, 1, di))
                    throw new InvalidOperationException("StartDocPrinter thất bại.");

                try
                {
                    if (!StartPagePrinter(hPrinter))
                        throw new InvalidOperationException("StartPagePrinter thất bại.");

                    try
                    {
                        IntPtr pUnmanaged = Marshal.AllocHGlobal(bytes.Length);
                        try
                        {
                            Marshal.Copy(bytes, 0, pUnmanaged, bytes.Length);
                            if (!WritePrinter(hPrinter, pUnmanaged, bytes.Length, out _))
                                throw new InvalidOperationException("WritePrinter thất bại.");
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(pUnmanaged);
                        }
                    }
                    finally
                    {
                        EndPagePrinter(hPrinter);
                    }
                }
                finally
                {
                    EndDocPrinter(hPrinter);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ClosePrinter(hPrinter);
            }
        }

        public static void PrintInvoiceToUsbPrinter(string printerName, Invoice invoice)
        {
            var bytes = InvoiceEscPosRenderer.BuildReceiptBytes(invoice, charsPerLine: 48);
            RawPrinterHelper.SendBytes(printerName, bytes, docName: "HoaDon_" + invoice.InvoiceNo);
        }
    }
}
