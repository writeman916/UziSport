using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UziSport.Controls;
using UziSport.Model;

namespace UziSport.DAL
{
    public class StockOutDetailDAL
    {
        private SQLiteAsyncConnection database;

        async Task Init()
        {
            if (database is not null)
                return;

            database = new SQLiteAsyncConnection(DBConstants.DatabasePath, DBConstants.Flags);

            var result = await database.CreateTableAsync<StockOutDetailInfo>();
        }

        public async Task<List<StockOutDetailViewInfo>> GetDetailByStockOutIdAsync(int stockOutId)
        {
            await Init();

            var sql = @$"
                SELECT
                    sd.StockOutDetailId,
                    sd.StockOutId,
                    sd.ProductId,
                    p.ProductCode,
                    p.ProductName,
                    b.BrandName,
                    c.CatalogName,  
                    p.Specification,
                    p.Cost,
                    p.Price,    
                    sd.Quantity,
                    sd.UnitCost,
                    sd.UnitPrice,
                    sd.LineDiscountAmount,
                    sd.CreateBy,
                    sd.CreateAt,
                    sd.UpdateBy,    
                    sd.UpdateAt
                FROM StockOutDetailInfo sd
                LEFT JOIN ProductInfo p ON sd.ProductId = p.ProductId
                LEFT JOIN BrandInfo b ON p.BrandId = b.BrandId
                LEFT JOIN CatalogInfo c ON p.CatalogId = c.CatalogId
                WHERE sd.StockOutId = {stockOutId}
                ORDER BY sd.CreateAt, sd.UpdateAt;
            ";

            var list = await database.QueryAsync<StockOutDetailViewInfo>(sql);

            return list;
        }

        public void SaveItemInTransaction(SQLiteConnection conn, List<StockOutDetailViewInfo> infos)
        {
            if (conn == null)
                throw new ArgumentNullException(nameof(conn));

            if (infos == null || infos.Count == 0)
                return;

            var newItems = infos
                .Where(s => s.StockOutDetailId == 0 && !s.Deleted)
                .ToList();

            var updateItems = infos
                .Where(s => s.StockOutDetailId != 0 && !s.Deleted)
                .ToList();

            var deleteItems = infos
                .Where(s => s.StockOutDetailId != 0 && s.Deleted)
                .ToList();

            // Insert
            foreach (var viewItem in newItems)
            {
                var entity = viewItem.ToStockOutDetailInfo();
                entity.CreateAt = DateTime.Now;
                entity.CreateBy = Constants.AdminCode;
                conn.Insert(entity);
                viewItem.StockOutDetailId = entity.StockOutDetailId;
            }

            // Update
            foreach (var viewItem in updateItems)
            {
                var entity = viewItem.ToStockOutDetailInfo();
                entity.CreateAt = DateTime.Now;
                entity.CreateBy = Constants.AdminCode;
                conn.Update(entity);
            }

            // Delete
            foreach (var viewItem in deleteItems)
            {
                conn.Execute("DELETE FROM StockOutDetailInfo WHERE StockOutDetailId = ?", viewItem.StockOutDetailId);
            }
        }

        public void DeleteByStockOutIdInTransaction(SQLiteConnection conn, int stockOutId)
        {
            if (conn == null)
                throw new ArgumentNullException(nameof(conn));

            conn.Execute("DELETE FROM StockOutDetailInfo WHERE StockOutId = ?", stockOutId);
        }

        public async Task DeleteByStockOutIdAsync(int stockOutId)
        {
            await Init();

            await database.ExecuteAsync(
                "DELETE FROM StockOutDetailInfo WHERE StockOutId = ?",
                stockOutId);
        }

    }
}
