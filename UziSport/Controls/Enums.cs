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

    public enum  PaymentMethod
    {
        None = 0,
        Cash = 1, //tien mat
        Transfer = 2 //chuyen khoan
    }

    public enum  PaymentStatus
    {
        None = 0,
        Unpaid = 1, //chua thanh toan
        Paid = 2, //da thanh toan
    }

    public static class Constants
    {
        public const string DateFormat = "dd/MM/yyyy";

        public const string AdminCode = "UZI";

        public const string PaymentMethod_Cash = "Tiền mặt";

        public const string PaymentMethod_Transfer = "Chuyển khoản";

        public const string PaymentStatus_Unpaid = "Chưa thanh toán";

        public const string PaymentStatus_Paid = "Đã thanh toán";
    }
}
