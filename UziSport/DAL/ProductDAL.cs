using Microsoft.VisualBasic;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UziSport.Model;

namespace UziSport.DAL
{
    public class ProductDAL
    {
        private SQLiteAsyncConnection database;

        async Task Init()
        {
            if (database is not null)
                return;

            database = new SQLiteAsyncConnection(DBConstants.DatabasePath, DBConstants.Flags);

            var result = await database.CreateTableAsync<ProductInfo>();
            await database.CreateTableAsync<CatalogInfo>();
            await database.CreateTableAsync<BrandInfo>();

        }

        public async Task<ProductInfo> GetItemByIdAsync(int productId)
        {
            await Init();

            try
            {
                return await database.Table<ProductInfo>()
                                     .Where(x => x.ProductId == productId)
                                     .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<List<ProductInfo>> GetProductsAsync()
        {
            await Init();

            var sql = @"
                SELECT 
                    p.ProductId,
                    p.ProductCode,
                    p.ProductName,
                    p.CatalogId,
                    c.CatalogName AS CatalogName,
                    p.BrandId,
                    b.BrandName AS BrandName,
                    p.Specification,
                    p.Cost,
                    p.Price,
                    p.Status,
                    p.Note,
                    p.CreatedBy,
                    p.CreatedAt,
                    p.UpdatedBy,    
                    p.UpdatedAt
                FROM ProductInfo p
                LEFT JOIN BrandInfo b ON p.BrandId = b.BrandId
                LEFT JOIN CatalogInfo c ON p.CatalogId = c.CatalogId
                ORDER BY p.ProductName;
            ";

            var list = await database.QueryAsync<ProductInfo>(sql);
            return list;
        }

        public async Task<ProductInfo?> GetProductByCodeAsync(string code)
        {
            await Init();

            var sql = @$"
                SELECT 
                    p.ProductId,
                    p.ProductCode,
                    p.ProductName,
                    p.CatalogId,
                    c.CatalogName AS CatalogName,
                    p.BrandId,
                    b.BrandName AS BrandName,
                    p.Specification,
                    p.Cost,
                    p.Price,
                    p.Status,
                    p.Note,
                    p.CreatedBy,
                    p.CreatedAt,
                    p.UpdatedBy,    
                    p.UpdatedAt
                FROM ProductInfo p
                LEFT JOIN BrandInfo b ON p.BrandId = b.BrandId
                LEFT JOIN CatalogInfo c ON p.CatalogId = c.CatalogId
                ORDER BY p.ProductName
                WHERE p.ProductCode = '{code}';
            ";

            List<ProductInfo> list = await database.QueryAsync<ProductInfo>(sql);

            return list.FirstOrDefault();
        }


        public async Task<int> SaveItemAsync(ProductInfo item)
        {
            await Init();

            if (item.ProductId != 0)
            {
                var existInfo = await database.Table<ProductInfo>()
                                              .Where(x => x.ProductId == item.ProductId)
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

        public async Task<int> DeleteByIdAsync(int productId)
        {
            await Init();

            var item = await GetItemByIdAsync(productId);
            if (item == null)
                return 0;

            return await database.DeleteAsync(item);
        }
    }
}
