using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UziSport.Model;

namespace UziSport.DAL
{
    public class StockInDAL
    {
        private SQLiteAsyncConnection database;

        private StockInDetailDAL detailDAL = new StockInDetailDAL();

        async Task Init()
        {
            if (database is not null)
                return;

            database = new SQLiteAsyncConnection(DBConstants.DatabasePath, DBConstants.Flags);

            var result = await database.CreateTableAsync<StockInInfo>();
            await database.CreateTableAsync<StockInDetailInfo>();
        }

        public async Task<List<StockInViewInfo>> GetAllStockInAsync()
        {
            await Init();

            var sql = @$"
                SELECT
                    s.StockInId,
                    s.StockInCode,
                    s.SupplierId,
                    --sup.SupplierName,
                    --s.WarehouseId,
                    --wh.WarehouseName,
                    s.StockInDate,
                    s.TotalAmount,
                    s.Status,
                    s.Note,
                    s.CreateBy,
                    s.CreateAt,
                    s.UpdateBy,
                    s.UpdateAt
                FROM StockInInfo s
                --LEFT JOIN WarehouseInfo wh ON wh.WarehouseId = s.WarehouseId
                ORDER BY sd.CreateAt, sd.UpdateAt DESC;
            ";

            var list = await database.QueryAsync<StockInViewInfo>(sql);

            return list;
        }

        public async Task<StockInViewInfo> GetStockInByIdAsync(int stockInId)
        {
            await Init();
            var sql = @$"
                SELECT
                    s.StockInId,
                    s.StockInCode,
                    s.SupplierId,
                    --sup.SupplierName,
                    --s.WarehouseId,
                    --wh.WarehouseName,
                    s.StockInDate,
                    s.TotalAmount,
                    s.Status,
                    s.Note,
                    s.CreateBy,
                    s.CreateAt,
                    s.UpdateBy,
                    s.UpdateAt
                FROM StockInInfo s
                --LEFT JOIN WarehouseInfo wh ON wh.WarehouseId = s.WarehouseId
                WHERE s.StockInId = {stockInId}
                ORDER BY sd.CreateAt, sd.UpdateAt DESC;
            ";
            var item = (await database.QueryAsync<StockInViewInfo>(sql)).FirstOrDefault();
            return item;
        }

        public async Task<int> SaveItemAsync(StockInViewInfo viewItem)
        {
            await Init();

            StockInInfo item = viewItem.ToStockInInfo();

            int result = 0;

            await database.RunInTransactionAsync(async (conn) =>
            {
                if (item.StockInId != 0)
                {
                    await database.UpdateAsync(item);
                    result = item.StockInId;
                }
                else
                {
                    await database.InsertAsync(item);
                    result = item.StockInId;
                }


                if (viewItem.StockInDetailInfos != null && viewItem.StockInDetailInfos.Count > 0)
                {
                    
                    detailDAL.SaveItemInTransaction(conn, viewItem.StockInDetailInfos);
                }else
                {
                    detailDAL.DeleteByStockInIdInTransaction(conn, item.StockInId);
                }
            });

            return result;
        }

        public async Task<int> DeleteByIdAsync(StockInViewInfo viewItem)
        {
            await Init();

            StockInInfo item = viewItem.ToStockInInfo();

            detailDAL.DeleteByStockInIdInTransaction(database.GetConnection(), item.StockInId);

            return await database.DeleteAsync(item);
        }
    }
}
