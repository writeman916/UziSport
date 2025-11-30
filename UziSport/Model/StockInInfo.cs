using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UziSport.Model
{
    public class StockInInfo : BaseModelInfo
    {
        [PrimaryKey, AutoIncrement]
        public int StockInId { get; set; }

        public string StockInCode { get; set; } = string.Empty;

        public int SupplierId { get; set; }

        public int WarehouseId { get; set; }

        public DateTime StockInDate { get; set; }

        public decimal? TotalAmount { get; set; }

        public int Status { get; set; }

        public string Note { get; set; } = string.Empty;
    }


    public class StockInViewInfo : StockInInfo
    {
        public string SupplierName { get; set; } = string.Empty;

        public string WarehouseName { get; set; } = string.Empty;

        public List<StockInDetailViewInfo> StockInDetailInfos { get; set; } = new List<StockInDetailViewInfo>();

        public StockInInfo ToStockInInfo()
        {
            return new StockInInfo
            {
                StockInId = this.StockInId,
                StockInCode = this.StockInCode,
                SupplierId = this.SupplierId,
                WarehouseId = this.WarehouseId,
                StockInDate = this.StockInDate,
                TotalAmount = this.TotalAmount,
                Status = this.Status,
                Note = this.Note,
                CreateAt = this.CreateAt,
                CreateBy = this.CreateBy,
                UpdateAt = this.UpdateAt,
                UpdateBy = this.UpdateBy
            };
        }
    } 
}
