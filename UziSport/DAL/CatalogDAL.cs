using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UziSport.Model;

namespace UziSport.DAL
{
    public class CatalogDAL
    {
        private SQLiteAsyncConnection database;

        async Task Init()
        {
            if (database is not null)
                return;

            database = new SQLiteAsyncConnection(DBConstants.DatabasePath, DBConstants.Flags);

            var result = await database.CreateTableAsync<CatalogInfo>();
        }

        public async Task<List<CatalogInfo>> GetCatalogsAsync()
        {
            await Init();
            return await database.Table<CatalogInfo>()
                                 .OrderBy(b => b.CatalogName)
                                 .ToListAsync();
        }

        public async Task<int> SaveItemAsync(CatalogInfo item)
        {
            await Init();

            if (item.CatalogId != 0)
            {
                var existInfo = await database.Table<CatalogInfo>()
                                              .Where(x => x.CatalogId == item.CatalogId)
                                              .FirstOrDefaultAsync();

                if (existInfo != null)
                {
                    return await database.UpdateAsync(item);
                }
            }

            return await database.InsertAsync(item);
        }

        public async Task<int> DeleteItemAsync(ProductInfo item)
        {
            await Init();
            return await database.DeleteAsync(item);
        }
    }
}
