using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UziSport.Model
{
    public class SupplierInfo
    {
        [PrimaryKey, AutoIncrement]

        public int SupplierId { get; set; }

        public string SupplierName { get; set; } = string.Empty;
    }
}
