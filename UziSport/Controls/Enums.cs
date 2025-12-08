using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UziSport.Controls
{
    public enum ImportStatus
    {
        InProgress = 1, // Đang xử lý
        Completed = 2,  // Hoàn thành
        Cancelled = 3   // Hủy
    }

    public static class Constants
    {
        public const string DateFormat = "dd/MM/yyyy";

        public const string AdminCode = "UZI";
    }
}
