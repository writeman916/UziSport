using SQLite;
using System;

namespace UziSport.Model
{
    public class AdminPasswordInfo
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string PasswordHash { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; }
    }
}
