using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UziSport.Model;

namespace UziSport.DAL
{
    public class StockInDetailDAL
    {
        private SQLiteAsyncConnection database;

        async Task Init()
        {
            if (database is not null)
                return;

            database = new SQLiteAsyncConnection(DBConstants.DatabasePath, DBConstants.Flags);

            var result = await database.CreateTableAsync<StockInDetailInfo>();
        }

        public async Task<List<StockInDetailViewInfo>> GetDetailByStockInIdAsync(int stockInId)
        {
            await Init();

            var sql = @$"
                SELECT
                    sd.StockInDetailId,
                    sd.StockInId,
                    sd.ProductId,
                    p.ProductName,
                    p.Specification,
                    p.Cost,
                    p.Price,    
                    sd.Quantity,
                    sd.UnitCost,
                    sd.Note,
                    sd.CreateBy,
                    sd.CreateAt,
                    sd.UpdateBy,    
                    sd.UpdateAt
                FROM StockInDetailInfo sd
                LEFT JOIN ProductInfo p ON sd.ProductId = p.ProductId
                WHERE sd.StockInId = {stockInId}
                ORDER BY sd.CreateAt, sd.UpdateAt;
            ";

            var list = await database.QueryAsync<StockInDetailViewInfo>(sql);

            return list;
        }

        public void SaveItemInTransaction(SQLiteConnection conn, List<StockInDetailViewInfo> infos)
        {
            if (infos == null || infos.Count == 0)
                return;


            var insertInfos = infos.Where(s => s.StockInDetailId == 0).Select(i => i.ToStockInDetailInfo()).ToList();
            var updateInfos = infos.Where(s => s.StockInDetailId != 0).Select(i => i.ToStockInDetailInfo()).ToList();


            if(insertInfos.Count > 0)
            {
                conn.InsertAll(insertInfos);
            }

            if(updateInfos.Count > 0)
            {
                foreach(var info in updateInfos)
                {
                    conn.Update(info);
                }
            }   
        }

        public void DeleteByStockInIdInTransaction(SQLiteConnection conn, int stockInId)
        {
            conn.Execute("DELETE FROM StockInDetailInfo WHERE StockInId = ?", stockInId);
        }

        public void DeleteByStockDetailIdInTransaction(SQLiteConnection conn, int stockDetailId)
        {
            conn.Execute("DELETE FROM StockInDetailInfo WHERE StockDetailId = ?", stockDetailId);
        }
    }
}
