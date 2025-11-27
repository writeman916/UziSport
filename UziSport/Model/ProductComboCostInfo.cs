using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UziSport.Model
{
    public class ProductComboCostInfo : BaseModelInfo
    {
        [PrimaryKey, AutoIncrement]
        public int ProductComboCostId { get; set; }

        public int ProductId { get; set; }

        public int StockDetailId { get; set; }

        public decimal Cost { get; set; }
    }
}
