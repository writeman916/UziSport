using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UziSport.Model
{
    public class CatalogInfo
    {
        [PrimaryKey, AutoIncrement]

        public int CatalogId { get; set; }

        public String CatalogName { get; set; } = string.Empty;
    }
}
