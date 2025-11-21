using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UziSport.Model
{
    public class BrandInfo
    {
        [PrimaryKey, AutoIncrement]

        public int BrandId { get; set; }

        public String BrandName { get; set; } = string.Empty;
    }
}
