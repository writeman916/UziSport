using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UziSport.Model;

namespace UziSport.DAL
{
    public class WarehouseDetailDAL
    {
        private SQLiteAsyncConnection database;

        private ProductComboCostDAL comboCostDAL = new ProductComboCostDAL();

        async Task Init()
        {
            if (database is not null)
                return;

            database = new SQLiteAsyncConnection(DBConstants.DatabasePath, DBConstants.Flags);

            var result = await database.CreateTableAsync<WarehouseDetailInfo>();
        }

        public async Task SaveItemAsync(List<StockInDetailViewInfo> infos, int warehouseId)
        {
            if (infos == null || infos.Count == 0)
                return;

            await Init();

            await database.RunInTransactionAsync(conn =>
            {
                foreach (var viewItem in infos)
                {
                    var entity = viewItem.ToWarehouseDetailInfo(warehouseId);

                    conn.Insert(entity);
                }
            });
        }
    }
}
