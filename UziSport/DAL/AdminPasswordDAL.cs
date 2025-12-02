using Microsoft.VisualBasic;
using SQLite;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UziSport.Model;

namespace UziSport.DAL
{
    public class AdminPasswordDAL
    {
        private SQLiteAsyncConnection database;

        private async Task Init()
        {
            if (database != null)
                return;

            database = new SQLiteAsyncConnection(DBConstants.DatabasePath, DBConstants.Flags);
            await database.CreateTableAsync<AdminPasswordInfo>();

            // Nếu chưa có bản ghi nào thì tạo 1 bản ghi với mật khẩu mặc định 00000000
            var existing = await database.Table<AdminPasswordInfo>().FirstOrDefaultAsync();
            if (existing == null)
            {
                string defaultHash = ComputeSha256Hash("00000000");
                var info = new AdminPasswordInfo
                {
                    PasswordHash = defaultHash,
                    UpdatedAt = DateTime.UtcNow
                };

                await database.InsertAsync(info);
            }
        }

        /// <summary>
        /// Lấy thông tin mật khẩu admin (nếu chưa set sẽ trả về bản ghi mặc định).
        /// </summary>
        public async Task<AdminPasswordInfo> GetAdminPasswordInfoAsync()
        {
            await Init();
            return await database.Table<AdminPasswordInfo>().FirstOrDefaultAsync();
        }

        /// <summary>
        /// Lưu/Update mật khẩu admin. Truyền mật khẩu thường, hàm sẽ tự hash.
        /// </summary>
        public async Task<int> SaveAdminPasswordAsync(string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
                throw new ArgumentException("Mật khẩu không được rỗng.", nameof(plainPassword));

            await Init();

            string hash = ComputeSha256Hash(plainPassword);

            var existing = await database.Table<AdminPasswordInfo>().FirstOrDefaultAsync();

            if (existing == null)
            {
                var info = new AdminPasswordInfo
                {
                    PasswordHash = hash,
                    UpdatedAt = DateTime.UtcNow
                };

                return await database.InsertAsync(info);
            }
            else
            {
                existing.PasswordHash = hash;
                existing.UpdatedAt = DateTime.UtcNow;
                return await database.UpdateAsync(existing);
            }
        }

        /// <summary>
        /// Kiểm tra mật khẩu admin có đúng hay không.
        /// </summary>
        public async Task<bool> ValidateAdminPasswordAsync(string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
                return false;

            await Init();

            var existing = await database.Table<AdminPasswordInfo>().FirstOrDefaultAsync();
            if (existing == null)
                return false;

            string hash = ComputeSha256Hash(plainPassword);
            return string.Equals(existing.PasswordHash, hash, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Hàm hash mật khẩu SHA256 đơn giản.
        /// </summary>
        private static string ComputeSha256Hash(string rawData)
        {
            using (var sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                var builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
