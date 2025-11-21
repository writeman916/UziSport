using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UziSport.Model;

namespace UziSport.DAL
{
    public class BrandDAL
    {
        private SQLiteAsyncConnection database;

        async Task Init()
        {
            if (database is not null)
                return;

            database = new SQLiteAsyncConnection(DBConstants.DatabasePath, DBConstants.Flags);

            var result = await database.CreateTableAsync<BrandInfo>();
        }

        public async Task<List<BrandInfo>> GetBrandsAsync()
        {
            await Init();
            return await database.Table<BrandInfo>()
                                 .OrderBy(b => b.BrandName)
                                 .ToListAsync();
        }

        public async Task<int> SaveItemAsync(BrandInfo item)
        {
            await Init();

            if (item.BrandId != 0)
            {
                var existInfo = await database.Table<BrandInfo>()
                                              .Where(x => x.BrandId == item.BrandId)
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
