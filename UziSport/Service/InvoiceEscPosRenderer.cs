using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UziSport.Model;

namespace UziSport.Service
{
    public static class InvoiceEscPosRenderer
    {
        // 80mm paper, printable width 72mm -> 576 dots on most 203dpi ESC/POS printers
        private const ushort PrintableWidthDots = 576;

        // Font A normal thường là 48 ký tự / dòng trên vùng 576 dots
        private const int CharsPerLine72mm = 46;

        public static byte[] BuildReceiptBytes(Invoice inv, Bitmap? logo = null)
        {
            var b = new List<byte>();

            void Add(byte[] x) => b.AddRange(x);

            Add(EscPos.Init());

            // Thiết lập vùng in thực tế 72mm
            Add(EscPos.SetLeftMargin(0));
            Add(EscPos.SetPrintAreaWidth(PrintableWidthDots));
            Add(EscPos.AlignLeft());

            // ===== LOGO =====
            if (logo != null)
            {
                Add(EscPos.AlignCenter());
                Add(EscPosImage.RasterImage(logo));
                Add(EscPos.Text("\r\n"));
            }

            // ===== HEADER =====
            Add(EscPos.AlignCenter());
            Add(EscPos.BoldOn());
            Add(EscPos.DoubleOn());
            Add(EscPos.Text(ToPrintText(inv.ShopName) + "\n"));
            Add(EscPos.DoubleOff());
            Add(EscPos.BoldOff());

            if (!string.IsNullOrWhiteSpace(inv.ShopAddress))
                Add(EscPos.Text(ToPrintText(inv.ShopAddress) + "\n"));

            if (!string.IsNullOrWhiteSpace(inv.ShopPhone))
                Add(EscPos.Text("SDT: " + ToPrintText(inv.ShopPhone) + "\n"));
            
            Add(EscPos.AlignLeft());
            Add(EscPos.Text(EscPos.Line(CharsPerLine72mm)));

            // ===== INFO =====
            Add(EscPos.AlignLeft());

            Add(EscPos.Text(
                EscPos.PadLR(
                    "No: " + ToPrintText(inv.InvoiceNo),
                    inv.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    CharsPerLine72mm) + "\n"));

            if (!string.IsNullOrWhiteSpace(inv.Note))
            {
                foreach (var line in EscPos.Wrap(ToPrintText("GHI CHU: " + inv.Note), CharsPerLine72mm))
                    Add(EscPos.Text(line + "\n"));
            }

            Add(EscPos.Text(EscPos.Line(CharsPerLine72mm)));

            // ===== ITEMS HEADER =====
            // 48 ký tự tổng cộng
            // SP(20) + SL(6) + D.GIA(11) + T.TIEN(11) = 48
            const int wName = 18;
            const int wQty = 6;
            const int wPrice = 11;
            const int wTotal = 11;

            Add(EscPos.BoldOn());
            Add(EscPos.Text(
                Fix("SAN PHAM", wName) +
                Fix("SL", wQty, true) +
                Fix("D.GIA", wPrice, true) +
                Fix("T.TIEN", wTotal, true) + "\n"));
            Add(EscPos.BoldOff());

            // ===== ITEMS =====
            if (inv.Lines != null)
            {
                foreach (var item in inv.Lines)
                {
                    string productName = ToPrintText(item.Name);
                    List<string> nameLines = EscPos.Wrap(productName, wName).ToList();

                    if (nameLines.Count == 0)
                        nameLines.Add(string.Empty);

                    // Dòng đầu có đủ 4 cột
                    Add(EscPos.Text(
                        Fix(nameLines[0], wName) +
                        Fix(item.Qty.ToString(), wQty, true) +
                        Fix(FormatMoney(item.Price), wPrice, true) +
                        Fix(FormatMoney(item.Total), wTotal, true) + "\n"));
                    Add(EscPos.Text("\n"));

                    // Dòng sau chỉ in phần tên bị wrap
                    for (int i = 1; i < nameLines.Count; i++)
                    {
                        Add(EscPos.Text(Fix(nameLines[i], wName) + "\n"));
                    }
                }
            }

            Add(EscPos.Text(EscPos.Line(CharsPerLine72mm)));

            // ===== TOTALS =====
            Add(EscPos.AlignLeft());
            Add(EscPos.Text(EscPos.PadLR("Thanh Tien:", FormatMoney(inv.SubTotal), CharsPerLine72mm) + "\n"));

            if (inv.Discount != 0)
                Add(EscPos.Text(EscPos.PadLR("Giam Gia:", FormatMoney(inv.Discount), CharsPerLine72mm) + "\n"));

            Add(EscPos.BoldOn());
            Add(EscPos.Text(EscPos.PadLR("Tong:", FormatMoney(inv.GrandTotal), CharsPerLine72mm) + "\n"));
            Add(EscPos.BoldOff());

            //if (inv.Paid > 0)
            //{
            //    Add(EscPos.Text(EscPos.PadLR("Nhan vao:", FormatMoney(inv.Paid), CharsPerLine72mm) + "\n"));
            //    Add(EscPos.Text(EscPos.PadLR("Tien thua:", FormatMoney(inv.Change), CharsPerLine72mm) + "\n"));
            //}

            // ===== FOOTER =====
            Add(EscPos.AlignCenter());
            Add(EscPos.Feed(1));
            Add(EscPos.Text("Cam On Quy Khach!\n"));
            Add(EscPos.Feed(5));
            Add(EscPos.CutFull());

            return b.ToArray();
        }

        private static string Fix(string s, int w, bool right = false)
        {
            s = s ?? string.Empty;

            if (s.Length > w)
                s = s.Substring(0, w);

            return right ? s.PadLeft(w) : s.PadRight(w);
        }

        private static string FormatMoney(decimal v)
        {
            return v.ToString("#,0");
        }

        private static string ToPrintText(string? text)
        {
            text = text ?? string.Empty;
            text = text.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Trim();

            // Giữ độ rộng ổn định cho ESC/POS nếu máy không hỗ trợ Unicode/Vietnamese tốt
            return VietText.ToAsciiVietnamese(text);
        }
    }
}