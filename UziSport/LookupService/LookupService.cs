using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UziSport.Model;

namespace UziSport.LookupService
{
    public class LookupService : ILookupService
    {
        private readonly SQLite.SQLiteAsyncConnection _db;

        private IReadOnlyList<CatalogInfo>? _catalogCache;
        private bool _catalogLoaded = false;

        public LookupService(SQLite.SQLiteAsyncConnection db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<CatalogInfo>> GetCatalogsAsync(bool forceRefresh = false)
        {
            if (!_catalogLoaded || forceRefresh)
            {
                _catalogCache = await _db.Table<CatalogInfo>()
                                         .OrderBy(x => x.CatalogName)
                                         .ToListAsync();

                _catalogLoaded = true;
            }

            // _catalogCache lúc này chắc chắn != null
            return _catalogCache!;
        }
    }

}
