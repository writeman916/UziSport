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

    }

    public class ProductViewInfo : ProductInfo
    {
        public string CatalogName { get; set; } = string.Empty;

        public string BrandName { get; set; } = string.Empty;

        public List<ProductComboCostInfo> ProductComboCostInfos { get; set; } = new List<ProductComboCostInfo>();

        public string SearchResultString 
        {
            get 
            {
                return $"{ProductCode}|{ProductName}|{BrandName}|{CatalogName}|{Specification}";
            }
        }

        public ProductInfo ToProductInfo()
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
            };
        }

        public ProductViewInfo CloneProduct()
        {
            return new ProductViewInfo
            {
                ProductId = this.ProductId,
                ProductCode = this.ProductCode,
                ProductName = this.ProductName,
                CatalogId = this.CatalogId,
                CatalogName = this.CatalogName,
                BrandId = this.BrandId,
                BrandName = this.BrandName,
                Specification = this.Specification,
                Cost = this.Cost,
                Price = this.Price,
                Status = this.Status,
                Note = this.Note,

                CreateBy = this.CreateBy,
                CreateAt = this.CreateAt,
                UpdateBy = this.UpdateBy,
                UpdateAt = this.UpdateAt,

                ProductComboCostInfos = this.ProductComboCostInfos?
                    .Select(c => new ProductComboCostInfo
                    {
                        ProductComboCostId = c.ProductComboCostId,
                        ProductId = c.ProductId,
                        Cost = c.Cost,
                        CreateAt = c.CreateAt,
                        CreateBy = c.CreateBy,
                        UpdateAt = c.UpdateAt,
                        UpdateBy = c.UpdateBy
                    })
                    .ToList() ?? new List<ProductComboCostInfo>()
            };
        }
    }
}
