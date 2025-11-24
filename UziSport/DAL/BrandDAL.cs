using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UziSport.Model;

namespace UziSport.DAL
{
    public class BrandDAL
    {
        // SINGLETON
        public static BrandDAL Instance { get; } = new BrandDAL();

        private SQLiteAsyncConnection database;

        // CACHE trong RAM
        private List<BrandInfo> _brandCache;
        private bool _isBrandLoaded = false;

        async Task Init()
        {
            if (database is not null)
                return;

            database = new SQLiteAsyncConnection(DBConstants.DatabasePath, DBConstants.Flags);

            var result = await database.CreateTableAsync<BrandInfo>();
        }

        /// <summary>
        /// Lấy danh sách Brand.
        /// - Lần đầu: đọc từ DB, lưu cache.
        /// - Các lần sau: đọc từ cache.
        /// - Nếu forceRefresh = true: luôn đọc lại từ DB.
        /// </summary>
        public async Task<List<BrandInfo>> GetBrandsAsync(bool forceRefresh = false)
        {
            await Init();

            if (!_isBrandLoaded || forceRefresh || _brandCache == null)
            {
                _brandCache = await database.Table<BrandInfo>()
                                            .OrderBy(b => b.BrandName)
                                            .ToListAsync();
                _isBrandLoaded = true;
            }

            return _brandCache;
        }

        /// <summary>
        /// Thêm / sửa Brand.
        /// </summary>
        public async Task<int> SaveItemAsync(BrandInfo item)
        {
            await Init();

            int result;

            if (item.BrandId != 0)
            {
                var existInfo = await database.Table<BrandInfo>()
                                              .Where(x => x.BrandId == item.BrandId)
                                              .FirstOrDefaultAsync();

                if (existInfo != null)
                {
                    result = await database.UpdateAsync(item);

                    // Cập nhật cache nếu đã load
                    if (_isBrandLoaded && _brandCache != null)
                    {
                        var index = _brandCache.FindIndex(x => x.BrandId == item.BrandId);
                        if (index >= 0)
                            _brandCache[index] = item;
                    }

                    return result;
                }
            }

            // Insert mới
            result = await database.InsertAsync(item);

            // Sau Insert, SQLite sẽ set lại BrandId cho item (PK AutoIncrement)
            if (_isBrandLoaded && _brandCache != null)
            {
                _brandCache.Add(item);
            }

            return result;
        }

        /// <summary>
        /// Xóa Brand.
        /// </summary>
        public async Task<int> DeleteItemAsync(BrandInfo item)
        {
            await Init();

            int result = await database.DeleteAsync(item);

            if (_isBrandLoaded && _brandCache != null)
            {
                _brandCache.RemoveAll(x => x.BrandId == item.BrandId);
            }

            return result;
        }

        /// <summary>
        /// Xóa cache nếu cần reload lại từ DB.
        /// </summary>
        public void ClearCache()
        {
            _brandCache = null;
            _isBrandLoaded = false;
        }
    }
}
