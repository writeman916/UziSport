using System.Net.Sockets;
using UziSport.Model;

namespace UziSport.Service
{
    public static class LanEscPosPrinter
    {
        public static void Send(string ip, int port, byte[] data)
        {
            using (var client = new TcpClient())
            {
                client.Connect(ip, port);
                using (var ns = client.GetStream())
                {
                    ns.Write(data, 0, data.Length);
                    ns.Flush();
                }
            }
        }

        public static void PrintInvoiceToLan(string printerIp, Invoice invoice, int port = 9100)
        {
            var bytes = InvoiceEscPosRenderer.BuildReceiptBytes(invoice, charsPerLine: 48);
            LanEscPosPrinter.Send(printerIp, port, bytes);
        }
    }
}
