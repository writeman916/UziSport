using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UziSport.Model;

namespace UziSport.DAL
{
    public class SupplierDAL
    {
        // SINGLETON
        public static SupplierDAL Instance { get; } = new SupplierDAL();

        private SQLiteAsyncConnection database;

        // CACHE trong RAM
        private List<SupplierInfo> _supplierCache;
        private bool _isSupplierLoaded = false;

        async Task Init()
        {
            if (database is not null)
                return;

            database = new SQLiteAsyncConnection(DBConstants.DatabasePath, DBConstants.Flags);

            var result = await database.CreateTableAsync<SupplierInfo>();
        }

        /// <summary>
        /// Lấy danh sách Supplier.
        /// - Lần đầu: đọc từ DB, lưu cache.
        /// - Các lần sau: đọc từ cache.
        /// - Nếu forceRefresh = true: luôn đọc lại từ DB.
        /// </summary>
        public async Task<List<SupplierInfo>> GetSuppliersAsync(bool forceRefresh = false)
        {
            await Init();

            if (!_isSupplierLoaded || forceRefresh || _supplierCache == null)
            {
                _supplierCache = await database.Table<SupplierInfo>()
                                            .OrderBy(b => b.SupplierName)
                                            .ToListAsync();
                _isSupplierLoaded = true;
            }

            return _supplierCache;
        }

        /// <summary>
        /// Thêm / sửa Supplier.
        /// </summary>
        public async Task<int> SaveItemAsync(SupplierInfo item)
        {
            await Init();

            int result;

            if (item.SupplierId != 0)
            {
                var existInfo = await database.Table<SupplierInfo>()
                                              .Where(x => x.SupplierId == item.SupplierId)
                                              .FirstOrDefaultAsync();

                if (existInfo != null)
                {
                    result = await database.UpdateAsync(item);

                    // Cập nhật cache nếu đã load
                    if (_isSupplierLoaded && _supplierCache != null)
                    {
                        var index = _supplierCache.FindIndex(x => x.SupplierId == item.SupplierId);
                        if (index >= 0)
                            _supplierCache[index] = item;
                    }

                    return result;
                }
            }

            // Insert mới
            result = await database.InsertAsync(item);

            // Sau Insert, SQLite sẽ set lại SupplierId cho item (PK AutoIncrement)
            if (_isSupplierLoaded && _supplierCache != null)
            {
                _supplierCache.Add(item);
            }

            return result;
        }

        /// <summary>
        /// Xóa Supplier.
        /// </summary>
        public async Task<int> DeleteItemAsync(SupplierInfo item)
        {
            await Init();

            int result = await database.DeleteAsync(item);

            if (_isSupplierLoaded && _supplierCache != null)
            {
                _supplierCache.RemoveAll(x => x.SupplierId == item.SupplierId);
            }

            return result;
        }

        /// <summary>
        /// Xóa cache nếu cần reload lại từ DB.
        /// </summary>
        public void ClearCache()
        {
            _supplierCache = null;
            _isSupplierLoaded = false;
        }
    }
}
