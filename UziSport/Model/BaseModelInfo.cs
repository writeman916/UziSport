using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UziSport.Model
{
    public class BaseModelInfo
    {
        public DateTime CreateAt { get; set; }

        public String CreateBy { get; set; } = string.Empty;

        public DateTime UpdateAt { get; set; }

        public string UpdateBy { get; set; } = string.Empty;

    }
}
