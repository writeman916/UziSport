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
        // SINGLETON
        public static CatalogDAL Instance { get; } = new CatalogDAL();

        private SQLiteAsyncConnection database;

        // CACHE trong RAM
        private List<CatalogInfo> _catalogCache;
        private bool _isCatalogLoaded = false;

        async Task Init()
        {
            if (database is not null)
                return;

            database = new SQLiteAsyncConnection(DBConstants.DatabasePath, DBConstants.Flags);

            var result = await database.CreateTableAsync<CatalogInfo>();
        }

        public async Task<List<CatalogInfo>> GetCatalogsAsync(bool forceRefresh = false)
        {
            await Init();

            if (!_isCatalogLoaded || forceRefresh || _catalogCache == null)
            {
                _catalogCache = await database.Table<CatalogInfo>()
                                              .OrderBy(b => b.CatalogName)
                                              .ToListAsync();
                _isCatalogLoaded = true;
            }

            return _catalogCache;
        }

        public async Task<int> SaveItemAsync(CatalogInfo item)
        {
            await Init();

            int result;

            if (item.CatalogId != 0)
            {
                var existInfo = await database.Table<CatalogInfo>()
                                              .Where(x => x.CatalogId == item.CatalogId)
                                              .FirstOrDefaultAsync();

                if (existInfo != null)
                {
                    result = await database.UpdateAsync(item);

                    if (_isCatalogLoaded && _catalogCache != null)
                    {
                        var index = _catalogCache.FindIndex(x => x.CatalogId == item.CatalogId);
                        if (index >= 0)
                            _catalogCache[index] = item;
                    }

                    return result;
                }
            }

            result = await database.InsertAsync(item);

            if (_isCatalogLoaded && _catalogCache != null)
            {
                _catalogCache.Add(item);
            }

            return result;
        }

        public async Task<int> DeleteItemAsync(CatalogInfo item)
        {
            await Init();

            int result = await database.DeleteAsync(item);

            if (_isCatalogLoaded && _catalogCache != null)
            {
                _catalogCache.RemoveAll(x => x.CatalogId == item.CatalogId);
            }

            return result;
        }

        public void ClearCache()
        {
            _catalogCache = null;
            _isCatalogLoaded = false;
        }
    }
}
