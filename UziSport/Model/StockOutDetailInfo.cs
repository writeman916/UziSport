using SQLite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UziSport.Model
{
    public class StockOutDetailInfo : BaseModelInfo
    {
        [PrimaryKey, AutoIncrement]
        public int StockOutDetailId { get; set; }

        public int StockOutId { get; set; }

        public int ProductId { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal UnitCost { get; set; }

        public decimal LineDiscountAmount { get; set; }
    }

    public class StockOutDetailViewInfo : StockOutDetailInfo
    {
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Specification { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public string CatalogName { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
        public decimal? SubTotal
        {
            get
            {
                return (this.UnitPrice * this.Quantity) - this.LineDiscountAmount;
            }
        }

        public List<ProductComboCostInfo> ProductComboCostInfos { get; set; } = new List<ProductComboCostInfo>();

        public bool Deleted { get; set; } = false;
        public decimal LineSaleAmount
        {
            get
            {
                decimal price = Price;

                return (decimal)(price * Quantity);
            }
        }

        public decimal LineAfterDiscountSaleAmout
        {
            get
            {
                return LineSaleAmount - LineDiscountAmount;
            }
        }

        public string QuantityString
        {
            get
            {
                return $"x {this.Quantity}";
            }
        }

        public string LineDiscountAmountString
        {
            get
            {
                if (this.LineDiscountAmount == 0)
                    return string.Empty;

                return $"- {this.LineDiscountAmount.ToString("N0", CultureInfo.InvariantCulture)}";

            }
        }

        public StockOutDetailInfo ToStockOutDetailInfo()
        {
            return new StockOutDetailInfo
            {
                StockOutDetailId = this.StockOutDetailId,
                StockOutId = this.StockOutId,
                ProductId = this.ProductId,
                Quantity = this.Quantity,
                UnitPrice = this.UnitPrice,
                UnitCost = this.UnitCost,
                LineDiscountAmount = this.LineDiscountAmount
            };
        }
    }
}
