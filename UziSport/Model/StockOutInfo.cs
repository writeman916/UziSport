using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UziSport.Model
{
    public class StockOutInfo : BaseModelInfo
    {
        [PrimaryKey, AutoIncrement]
        public int StockOutId { get; set; }

        public string StockOutCode { get; set; } = string.Empty;

        public DateTime StockOutDate { get; set; }

        public int? CustomerId { get; set; }

        public string Note { get; set; } = string.Empty;

        public decimal InvoiceDiscountAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public int PaymentMethod { get; set; }

        public decimal ActualIncome { get; set; }
    
    }

    public  class StockOutViewInfo : StockOutInfo
    {
        public string CustomerName { get; set; } = string.Empty;
        public List<StockOutDetailViewInfo> StockOutDetailInfos { get; set; } = new List<StockOutDetailViewInfo>();

        public decimal InvoiceDiscountRate { get; set; }

        public bool Deleted { get; set; } = false;

        public StockOutInfo ToStockOutInfo()
        {
            return new StockOutInfo
            {
                StockOutId = this.StockOutId,
                StockOutCode = this.StockOutCode,
                StockOutDate = this.StockOutDate,
                CustomerId = this.CustomerId,
                ActualIncome = this.ActualIncome,
                Note = this.Note,
                InvoiceDiscountAmount = this.InvoiceDiscountAmount,
                TotalAmount = this.TotalAmount,
                PaymentMethod = this.PaymentMethod,
                CreateBy = this.CreateBy,
                CreateAt = this.CreateAt,
                UpdateBy = this.UpdateBy,
                UpdateAt = this.UpdateAt
            };
        }
    }
}
