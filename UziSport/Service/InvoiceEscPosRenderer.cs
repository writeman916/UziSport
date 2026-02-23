using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UziSport.Model;

namespace UziSport.Service
{
    public static class InvoiceEscPosRenderer
    {
        public static byte[] BuildReceiptBytes(Invoice inv, int charsPerLine = 48)
        {
            var b = new List<byte>();

            void Add(byte[] x) => b.AddRange(x);

            Add(EscPos.Init());

            // HEADER
            Add(EscPos.AlignCenter());
            Add(EscPos.BoldOn());
            Add(EscPos.DoubleOn());
            Add(EscPos.Text(inv.ShopName + "\n"));
            Add(EscPos.DoubleOff());
            Add(EscPos.BoldOff());

            if (!string.IsNullOrWhiteSpace(inv.ShopAddress))
                Add(EscPos.Text(inv.ShopAddress + "\n"));
            if (!string.IsNullOrWhiteSpace(inv.ShopPhone))
                Add(EscPos.Text("ĐT: " + inv.ShopPhone + "\n"));

            Add(EscPos.Text(EscPos.Line(charsPerLine)));

            // INFO
            Add(EscPos.AlignLeft());
            Add(EscPos.Text(EscPos.PadLR("HĐ: " + inv.InvoiceNo, inv.CreatedAt.ToString("dd/MM/yyyy HH:mm"), charsPerLine) + "\n"));

            if (!string.IsNullOrWhiteSpace(inv.CustomerName))
                Add(EscPos.Text("KH: " + inv.CustomerName + "\n"));
            if (!string.IsNullOrWhiteSpace(inv.Note))
            {
                foreach (var line in EscPos.Wrap("Ghi chú: " + inv.Note, charsPerLine))
                    Add(EscPos.Text(line + "\n"));
            }

            Add(EscPos.Text(EscPos.Line(charsPerLine)));

            // ITEMS HEADER
            // Cột gợi ý cho 80mm: Tên(24) | SL(4) | Giá(9) | TT(11) = 48
            int wName = 24, wQty = 4, wPrice = 9, wTotal = 11;

            Add(EscPos.BoldOn());
            Add(EscPos.Text(
                (Fix("Sản phẩm", wName) +
                 Fix("SL", wQty, true) +
                 Fix("Giá", wPrice, true) +
                 Fix("T.Tiền", wTotal, true)) + "\n"));
            Add(EscPos.BoldOff());

            // ITEMS
            foreach (var line in inv.Lines)
            {
                var nameLines = EscPos.Wrap(line.Name, wName).ToList();
                if (nameLines.Count == 0) nameLines.Add("");

                // dòng 1 có đủ cột
                Add(EscPos.Text(
                    Fix(nameLines[0], wName) +
                    Fix(line.Qty.ToString(), wQty, true) +
                    Fix(FormatMoney(line.Price), wPrice, true) +
                    Fix(FormatMoney(line.Total), wTotal, true) + "\n"));

                // các dòng name bọc xuống chỉ in tên
                for (int i = 1; i < nameLines.Count; i++)
                    Add(EscPos.Text(Fix(nameLines[i], wName) + "\n"));
            }

            Add(EscPos.Text(EscPos.Line(charsPerLine)));

            // TOTALS
            Add(EscPos.AlignRight());
            Add(EscPos.Text(EscPos.PadLR("Tạm tính:", FormatMoney(inv.SubTotal), charsPerLine) + "\n"));
            if (inv.Discount != 0)
                Add(EscPos.Text(EscPos.PadLR("Giảm giá:", FormatMoney(inv.Discount), charsPerLine) + "\n"));

            Add(EscPos.BoldOn());
            Add(EscPos.Text(EscPos.PadLR("TỔNG CỘNG:", FormatMoney(inv.GrandTotal), charsPerLine) + "\n"));
            Add(EscPos.BoldOff());

            if (inv.Paid > 0)
            {
                Add(EscPos.Text(EscPos.PadLR("Khách đưa:", FormatMoney(inv.Paid), charsPerLine) + "\n"));
                Add(EscPos.Text(EscPos.PadLR("Tiền thừa:", FormatMoney(inv.Change), charsPerLine) + "\n"));
            }

            // FOOTER
            Add(EscPos.AlignCenter());
            Add(EscPos.Feed(1));
            Add(EscPos.Text("Cảm ơn Quý khách!\n"));
            Add(EscPos.Feed(2));
            Add(EscPos.CutFull());

            return b.ToArray();

            static string Fix(string s, int w, bool right = false)
            {
                s ??= "";
                if (s.Length > w) s = s.Substring(0, w);
                return right ? s.PadLeft(w) : s.PadRight(w);
            }

            static string FormatMoney(decimal v)
            {
                // Bạn có thể đổi format theo ý: "#,0" hoặc "#,0 đ"
                return v.ToString("#,0");
            }
        }
    }
}
