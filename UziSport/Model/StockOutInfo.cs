using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using UziSport.Controls;

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

        public int PaymentStatus { get; set; }

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
                PaymentStatus = this.PaymentStatus,
                CreateBy = this.CreateBy,
                CreateAt = this.CreateAt,
                UpdateBy = this.UpdateBy,
                UpdateAt = this.UpdateAt
            };
        }
    }

    public class StockOutHistoryInfo : StockOutViewInfo
    {
        public string PaymentMethodName 
        {
            get
            {
                switch(this.PaymentMethod)
                {
                    case (int)Controls.PaymentMethod.Cash:
                        return Controls.Constants.PaymentMethod_Cash;
                    case (int)Controls.PaymentMethod.Transfer:
                        return Controls.Constants.PaymentMethod_Transfer;
                    default:
                        return string.Empty;
                }
            }
        }

        public string PaymentStatusName
        {
            get
            {
                switch (this.PaymentStatus)
                {
                    case (int)Controls.PaymentStatus.Paid:
                        return "Đã xong";
                    case (int)Controls.PaymentStatus.Unpaid:
                        return "Chưa xong";
                    default:
                        return string.Empty;
                }
            }
        }

        public decimal StockOutTotalCostAmount { get; set; }
        public decimal ProfitAmount { get; set; }
    }

    public class StockOutSearchCriteria
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string StockOutCode { get; set; } = string.Empty;
        public int? PaymentMethod { get; set; }
        public int? PaymentStatus { get; set; }
    }
}
