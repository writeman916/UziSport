using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UziSport.Model;

namespace UziSport.DAL
{
    public class WarehouseDAL
    {
        // SINGLETON
        public static WarehouseDAL Instance { get; } = new WarehouseDAL();

        private SQLiteAsyncConnection database;

        // CACHE trong RAM
        private List<WarehouseInfo> _warehouseCache;
        private bool _isWarehouseLoaded = false;

        async Task Init()
        {
            if (database is not null)
                return;

            database = new SQLiteAsyncConnection(DBConstants.DatabasePath, DBConstants.Flags);

            var result = await database.CreateTableAsync<WarehouseInfo>();
        }

        /// <summary>
        /// Lấy danh sách Warehouse.
        /// - Lần đầu: đọc từ DB, lưu cache.
        /// - Các lần sau: đọc từ cache.
        /// - Nếu forceRefresh = true: luôn đọc lại từ DB.
        /// </summary>
        public async Task<List<WarehouseInfo>> GetWarehousesAsync(bool forceRefresh = false)
        {
            await Init();

            if (!_isWarehouseLoaded || forceRefresh || _warehouseCache == null)
            {
                _warehouseCache = await database.Table<WarehouseInfo>()
                                            .OrderBy(b => b.WarehouseName)
                                            .ToListAsync();
                _isWarehouseLoaded = true;
            }

            return _warehouseCache;
        }

        /// <summary>
        /// Thêm / sửa Warehouse.
        /// </summary>
        public async Task<int> SaveItemAsync(WarehouseInfo item)
        {
            await Init();

            int result;

            if (item.WarehouseId != 0)
            {
                var existInfo = await database.Table<WarehouseInfo>()
                                              .Where(x => x.WarehouseId == item.WarehouseId)
                                              .FirstOrDefaultAsync();

                if (existInfo != null)
                {
                    result = await database.UpdateAsync(item);

                    // Cập nhật cache nếu đã load
                    if (_isWarehouseLoaded && _warehouseCache != null)
                    {
                        var index = _warehouseCache.FindIndex(x => x.WarehouseId == item.WarehouseId);
                        if (index >= 0)
                            _warehouseCache[index] = item;
                    }

                    return result;
                }
            }

            // Insert mới
            result = await database.InsertAsync(item);

            // Sau Insert, SQLite sẽ set lại WarehouseId cho item (PK AutoIncrement)
            if (_isWarehouseLoaded && _warehouseCache != null)
            {
                _warehouseCache.Add(item);
            }

            return result;
        }

        /// <summary>
        /// Xóa Warehouse.
        /// </summary>
        public async Task<int> DeleteItemAsync(WarehouseInfo item)
        {
            await Init();

            int result = await database.DeleteAsync(item);

            if (_isWarehouseLoaded && _warehouseCache != null)
            {
                _warehouseCache.RemoveAll(x => x.WarehouseId == item.WarehouseId);
            }

            return result;
        }

        /// <summary>
        /// Xóa cache nếu cần reload lại từ DB.
        /// </summary>
        public void ClearCache()
        {
            _warehouseCache = null;
            _isWarehouseLoaded = false;
        }
    }
}
