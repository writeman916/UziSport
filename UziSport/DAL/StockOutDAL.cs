using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using UziSport.Controls;
using UziSport.Model;

namespace UziSport.DAL
{
    public class StockOutDAL
    {
        private SQLiteAsyncConnection database;

        private StockOutDetailDAL stockOutDetailDAL = new StockOutDetailDAL();

        async Task Init()
        {
            if (database is not null)
                return;

            database = new SQLiteAsyncConnection(DBConstants.DatabasePath, DBConstants.Flags);

            var result = await database.CreateTableAsync<StockOutInfo>();
            await database.CreateTableAsync<StockOutDetailInfo>();
        }

        public async Task<int> SaveItemAsync(StockOutViewInfo viewItem)
        {
            if (viewItem == null)
                throw new ArgumentNullException(nameof(viewItem));

            await Init();

            int result = 0;

            await database.RunInTransactionAsync(conn =>
            {
                // Nếu đánh dấu xóa và đã có Id → xóa luôn
                if (viewItem.Deleted && viewItem.StockOutId != 0)
                {
                    stockOutDetailDAL.DeleteByStockOutIdInTransaction(conn, viewItem.StockOutId);
                    conn.Execute("DELETE FROM StockOutInfo WHERE StockOutId = ?", viewItem.StockOutId);
                    result = 1;
                    return;
                }

                // Map View → Entity header
                var header = viewItem.ToStockOutInfo();

                // Insert / Update header
                if (header.StockOutId == 0)
                {
                    header.CreateAt = DateTime.Now;
                    header.CreateBy = Constants.AdminCode;
                    result = conn.Insert(header);
                    viewItem.StockOutId = header.StockOutId;              
                }
                else
                {
                    header.UpdateAt = DateTime.Now;
                    header.UpdateBy = Constants.AdminCode;
                    result = conn.Update(header);

                    if (result == 0)
                    {
                        result = conn.Insert(header);
                    }
                }

                // Lưu detail
                if (viewItem.StockOutDetailInfos != null && viewItem.StockOutDetailInfos.Count > 0)
                {
                    foreach (var d in viewItem.StockOutDetailInfos)
                    {
                        d.StockOutId = header.StockOutId;
                    }

                    stockOutDetailDAL.SaveItemInTransaction(conn, viewItem.StockOutDetailInfos);
                }
                else
                {
                    // Nếu không có detail nào → xóa hết detail cũ (nếu có)
                    stockOutDetailDAL.DeleteByStockOutIdInTransaction(conn, header.StockOutId);
                }
            });

            return result;
        }

        public async Task<int> DeleteItemAsync(StockOutViewInfo viewItem)
        {
            if (viewItem == null)
                return 0;

            return await DeleteByIdAsync(viewItem.StockOutId);
        }

        public async Task<StockOutInfo> GetItemByIdAsync(int stockOutId)
        {
            await Init();

            return await database.Table<StockOutInfo>()
                                 .Where(x => x.StockOutId == stockOutId)
                                 .FirstOrDefaultAsync();
        }

        public async Task<int> DeleteByIdAsync(int stockOutId)
        {
            await Init();

            var header = await GetItemByIdAsync(stockOutId);

            if (header == null)
                return 0;

            await stockOutDetailDAL.DeleteByStockOutIdAsync(stockOutId);

            return await database.DeleteAsync(header);
        }
    }
}
