using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace UziSport.Model
{
    public class StockInDetailInfo : BaseModelInfo
    {
        public int StockInDetailId { get; set; }

        public int StockInId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitCost { get; set; }

        public string  Note { get; set; } = string.Empty;
    }

    public class StockInDetailViewInfo : StockInDetailInfo
    {
        public string ProductName { get; set; } = string.Empty;

        public decimal SubTotal  { get
            {
                return this.UnitCost * this.Quantity;
            }
        }
    }
}
