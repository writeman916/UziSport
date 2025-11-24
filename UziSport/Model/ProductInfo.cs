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

        public int BrandId { get; set; }

        public string Specification { get; set; } = string.Empty;

        public decimal Cost { get; set; }

        public decimal Price { get; set; }

        public int Status { get; set; }

        public string Note { get; set; } = string.Empty;

        [Ignore]
        public List<ProductComboCostInfo> ProductComboCostInfos { get; set; } = new List<ProductComboCostInfo>();

    }

    public class ProductViewInfo : BaseModelInfo
    {
        public int ProductId { get; set; }

        public string ProductCode { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public int CatalogId { get; set; }
        public string CatalogName { get; set; } = string.Empty;

        public int BrandId { get; set; }

        public string BrandName { get; set; } = string.Empty;

        public string Specification { get; set; } = string.Empty;

        public decimal Cost { get; set; }

        public decimal Price { get; set; }

        public int Status { get; set; }

        public string Note { get; set; } = string.Empty;

        public List<ProductComboCostInfo> ProductComboCostInfos { get; set; } = new List<ProductComboCostInfo>();

        public ProductInfo ConvertProductToSave()
        {
            return new ProductInfo()
            {
                ProductId = ProductId,
                ProductCode = ProductCode,
                ProductName = ProductName,
                CatalogId = CatalogId,
                BrandId = BrandId,
                Specification = Specification,
                Cost = Cost,
                Price = Price,
                Status = Status,
                Note = Note,
                CreateAt = CreateAt,
                CreateBy = CreateBy,
                UpdateAt = UpdateAt,
                UpdateBy = UpdateBy,
                ProductComboCostInfos = ProductComboCostInfos
            };
        }
    }
}
