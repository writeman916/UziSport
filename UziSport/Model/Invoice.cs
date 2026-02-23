using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UziSport.Model
{
    public class Invoice
    {
        public string ShopName { get; set; }
        public string ShopAddress { get; set; }
        public string ShopPhone { get; set; }

        public string InvoiceNo { get; set; }
        public DateTime CreatedAt { get; set; }

        public string CustomerName { get; set; }
        public string Note { get; set; }

        public List<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();

        public decimal SubTotal => Lines.Sum(x => x.Total);
        public decimal Discount { get; set; }      // giảm giá (tiền)
        public decimal GrandTotal => SubTotal - Discount;

        public decimal Paid { get; set; }          // khách đưa/đã thanh toán
        public decimal Change => Paid - GrandTotal;
    }

    public class InvoiceLine
    {
        public string Name { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
        public decimal Total => Qty * Price;
    }
}
