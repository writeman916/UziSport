using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UziSport.Model
{
    public class WarehouseInfo
    {
        [PrimaryKey, AutoIncrement]

        public int WarehouseId { get; set; }

        public string WarehouseName { get; set; } = string.Empty;

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }
    }
}
