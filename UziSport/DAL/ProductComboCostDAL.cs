using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using UziSport.Model;

namespace UziSport.DAL
{
    public class ProductComboCostDAL
    {
        private SQLiteAsyncConnection database;

        async Task Init()
        {
            if (database is not null)
                return;

            database = new SQLiteAsyncConnection(DBConstants.DatabasePath, DBConstants.Flags);

            var result = await database.CreateTableAsync<ProductComboCostInfo>();
        }

        public async Task<List<ProductComboCostInfo>> GetItemByProductIdAsync(int productId)
        {
            await Init();

            try
            {
                return await database.Table<ProductComboCostInfo>()
                                     .Where(x => x.ProductId == productId)
                                     .OrderBy(x => x.ProductComboCostId).ToListAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> SaveItemAsync(List<ProductComboCostInfo> infos)
        {
            if(infos == null || infos.Count == 0)
                return 0;

            await Init();

            int productId = infos[0].ProductId;

            await this.DeleteByProductIdAsync(productId);

            return await database.InsertAllAsync(infos);
        }

        public async Task<int> DeleteByProductIdAsync(int productId)
        {
            await Init();

            return await database.Table<ProductComboCostInfo>()
                                 .DeleteAsync(x => x.ProductId == productId);
        }

        internal void SaveItemInTransaction(SQLiteConnection conn, List<ProductComboCostInfo> infos)
        {
            if (infos == null || infos.Count == 0)
                return;

            int productId = infos[0].ProductId;

            conn.Execute("DELETE FROM ProductComboCostInfo WHERE ProductId = ?", productId);

            conn.InsertAll(infos);
        }

        internal void DeleteByProductIdInTransaction(SQLiteConnection conn, int productId)
        {
            conn.Execute("DELETE FROM ProductComboCostInfo WHERE ProductId = ?", productId);
        }
    }
}
