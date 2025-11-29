using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UziSport.Model
{
    public class WarehouseDetailInfo : BaseModelInfo
    {
        public int WarehouseDetailId { get; set; }

        public int WarehouseId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }
    }

    public class WarehouseDetailViewInfo : WarehouseDetailInfo
    {
        public string ProductName { get; set; } = string.Empty;

        public string BrandName { get; set; } = string.Empty;

        public string CatalogName { get; set; } = string.Empty;

        public WarehouseDetailInfo ToWarehouseDetailInfo()
        {
            return new WarehouseDetailInfo
            {
                WarehouseDetailId = this.WarehouseDetailId,
                WarehouseId = this.WarehouseId,
                ProductId = this.ProductId,
                Quantity = this.Quantity,
                CreateAt = this.CreateAt,
                CreateBy = this.CreateBy,
                UpdateAt = this.UpdateAt,
                UpdateBy = this.UpdateBy
            };
        }
    }
}
