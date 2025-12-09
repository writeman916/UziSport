using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UziSport.Model
{
    public class ProductInfo : BaseModelInfo
    {
        [PrimaryKey, AutoIncrement]
        public int ProductId { get; set; }

        public string ProductCode { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public int CatalogId { get; set; }

        public int BrandId { get; set; }

        public string Specification { get; set; } = string.Empty;

        public decimal? Cost { get; set; }

        public decimal? Price { get; set; }

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

    public class ProductStockViewInfo : ProductViewInfo, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
          => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public decimal StockQty { get; set; }

        private decimal _saleQty;
        public decimal SaleQty
        {
            get => _saleQty;
            set
            {
                if (_saleQty != value)
                {
                    _saleQty = value;
                    OnPropertyChanged(); // SaleQty
                    // Các property phụ thuộc:
                    OnPropertyChanged(nameof(LineSaleAmount));
                    OnPropertyChanged(nameof(LineDiscountAmount));
                    OnPropertyChanged(nameof(LineAfterDiscountSaleAmout));
                }
            }
        }

        public decimal LineDiscountRate { get; set; }
        public decimal LineDiscountAmount 
        {
            get
            {
                decimal price = this.Price ?? 0;
                return (price * LineDiscountRate / 100) * SaleQty;
            }
        }

        public string LineDiscountRateString {
            get 
            { 
                if(LineDiscountRate == 0)
                    return string.Empty;

                return this.LineDiscountRate.ToString() + "%"; 
            } 
        }

        public decimal AfterDiscountPrice
        {
            get
            {
                decimal price = this.Price ?? 0;
                return (price - (price * LineDiscountRate / 100));
            }
        }

        public bool IsComboCostPriority { get; set; } = true;

        public decimal LineAfterDiscountSaleAmout
        {
            get
            {
                decimal price = this.Price ?? 0;
                return (decimal)((price - (price * LineDiscountRate / 100)) * SaleQty); 
            }
        }

        public decimal LineSaleAmount
        {
            get
            {
                decimal price = this.Price ?? 0;
                return (decimal)(price * SaleQty);
            }
        }

        public ProductStockViewInfo Clone()
        {
            // Clone phần base (ProductViewInfo) – đã clone sâu ProductComboCostInfos
            var baseClone = this.CloneProduct();

            return new ProductStockViewInfo
            {
                // Base fields
                ProductId = baseClone.ProductId,
                ProductCode = baseClone.ProductCode,
                ProductName = baseClone.ProductName,
                CatalogId = baseClone.CatalogId,
                CatalogName = baseClone.CatalogName,
                BrandId = baseClone.BrandId,
                BrandName = baseClone.BrandName,
                Specification = baseClone.Specification,
                Cost = baseClone.Cost,
                Price = baseClone.Price,
                Status = baseClone.Status,
                Note = baseClone.Note,
                CreateBy = baseClone.CreateBy,
                CreateAt = baseClone.CreateAt,
                UpdateBy = baseClone.UpdateBy,
                UpdateAt = baseClone.UpdateAt,
                ProductComboCostInfos = baseClone.ProductComboCostInfos,

                // Fields riêng của ProductStockViewInfo
                TotalIn = this.TotalIn,
                TotalOut = this.TotalOut,
                StockQty = this.StockQty,
                SaleQty = this.SaleQty,
                LineDiscountRate = this.LineDiscountRate,
                IsComboCostPriority = this.IsComboCostPriority
            };
        }

    }
}
