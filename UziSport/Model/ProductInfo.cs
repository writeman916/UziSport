using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UziSport.Model
{
    public class ProductInfo : BaseModelInfo
    {
        [PrimaryKey, AutoIncrement]
        public int ProductId { get; set; }

        [Unique]
        public string ProductCode { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public int CatalogId { get; set; }
        [Ignore]
        public string CatalogName { get; set; } = string.Empty;

        public int BrandId { get; set; }
        [Ignore]
        public string BrandName { get; set; } = string.Empty;

        public string Specification { get; set; } = string.Empty;

        public decimal Cost { get; set; }

        public decimal Price { get; set; }

        public int Status { get; set; }

        public string Note { get; set; } = string.Empty;

        [Ignore]
        public List<Decimal> CostByComboList { get; set; } = new List<Decimal>();
    }
}
