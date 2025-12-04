using Microsoft.VisualBasic;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UziSport.Controls;
using UziSport.Model;

namespace UziSport.DAL
{
    public class ProductDAL
    {
        private SQLiteAsyncConnection database;

        private ProductComboCostDAL comboCostDAL = new ProductComboCostDAL();

        async Task Init()
        {
            if (database is not null)
                return;

            database = new SQLiteAsyncConnection(DBConstants.DatabasePath, DBConstants.Flags);

            var result = await database.CreateTableAsync<ProductInfo>();

            await database.CreateTableAsync<CatalogInfo>();
            await database.CreateTableAsync<BrandInfo>();
            await database.CreateTableAsync<ProductComboCostInfo>();
            await database.CreateTableAsync<StockOutDetailInfo>();
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

        public async Task<List<ProductViewInfo>> GetProductsAsync()
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
                    p.CreateBy,
                    p.CreateAt,
                    p.UpdateBy,    
                    p.UpdateAt
                FROM ProductInfo p
                LEFT JOIN BrandInfo b ON p.BrandId = b.BrandId
                LEFT JOIN CatalogInfo c ON p.CatalogId = c.CatalogId
                ORDER BY p.ProductName;
            ";

            var list = await database.QueryAsync<ProductViewInfo>(sql);

            return list;
        }

        public async Task<ProductViewInfo?> GetProductByCodeAsync(string code)
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
                    p.CreateBy,
                    p.CreateAt,
                    p.UpdateBy,    
                    p.UpdateAt
                FROM ProductInfo p
                LEFT JOIN BrandInfo b ON p.BrandId = b.BrandId
                LEFT JOIN CatalogInfo c ON p.CatalogId = c.CatalogId
                ORDER BY p.ProductName
                WHERE p.ProductCode = '{code}';
            ";

            List<ProductViewInfo> list = await database.QueryAsync<ProductViewInfo>(sql);

            return list.FirstOrDefault();
        }


        public async Task<int> SaveItemAsync(ProductViewInfo viewItem)
        {
            await Init();

            ProductInfo item = viewItem.ToProductInfo();

            int result = 0;

            await database.RunInTransactionAsync(conn =>
            {
                if (item.ProductId == 0)
                {
                    result = conn.Insert(item);
                }
                else
                {
                    result = conn.Update(item);

                    if (result == 0)
                    {
                        result = conn.Insert(item);
                    }
                }

                if (viewItem.ProductComboCostInfos != null && viewItem.ProductComboCostInfos.Count > 0)
                {
                    foreach (var cost in viewItem.ProductComboCostInfos)
                    {
                        cost.ProductId = item.ProductId;
                    }

                    comboCostDAL.SaveItemInTransaction(conn, viewItem.ProductComboCostInfos);
                }
                else
                {
                    comboCostDAL.DeleteByProductIdInTransaction(conn, item.ProductId);
                }
            });

            return result;
        }
        public async Task<List<ProductStockViewInfo>> GetProductsWithStockAsync()
        {
            await Init();

            var completedStatus = (int)ImportStatus.Completed; // = 2

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
                    p.CreateBy,
                    p.CreateAt,
                    p.UpdateBy,    
                    p.UpdateAt,
                    IFNULL(ins.TotalIn, 0)  AS TotalIn,
                    IFNULL(outs.TotalOut, 0) AS TotalOut,
                    IFNULL(ins.TotalIn, 0) - IFNULL(outs.TotalOut, 0) AS StockQty
                FROM ProductInfo p
                LEFT JOIN BrandInfo b   ON p.BrandId   = b.BrandId
                LEFT JOIN CatalogInfo c ON p.CatalogId = c.CatalogId
                LEFT JOIN (
                    SELECT 
                        d.ProductId,
                        SUM(d.Quantity) AS TotalIn
                    FROM StockInDetailInfo d
                    INNER JOIN StockInInfo h ON h.StockInId = d.StockInId
                    WHERE h.Status = ?
                    GROUP BY d.ProductId
                ) AS ins ON ins.ProductId = p.ProductId
                LEFT JOIN (
                    SELECT 
                        d.ProductId,
                        SUM(d.Quantity) AS TotalOut
                    FROM StockOutDetailInfo d
                    GROUP BY d.ProductId
                ) AS outs ON outs.ProductId = p.ProductId
                ORDER BY p.ProductName;
            ";

            // truyền completedStatus vào dấu ? trong SQL
            var list = await database.QueryAsync<ProductStockViewInfo>(sql, completedStatus);

            return list;
        }



        public async Task<int> DeleteItemAsync(ProductViewInfo viewItem)
        {
            await Init();

            ProductInfo item = viewItem.ToProductInfo();

            await comboCostDAL.DeleteByProductIdAsync(item.ProductId);

            return await database.DeleteAsync(item);
        }

        public async Task<int> DeleteByIdAsync(int productId)
        {
            await Init();

            var item = await GetItemByIdAsync(productId);

            if (item == null)
                return 0;

            await comboCostDAL.DeleteByProductIdAsync(productId);

            return await database.DeleteAsync(item);
        }
    }
}
