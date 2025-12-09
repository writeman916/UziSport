using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UziSport.Controls;

namespace UziSport.Model
{
    public class PaymentMethodInfo
    {
        public PaymentMethod Method { get; set; }

        public string MethodName { get; set; } = string.Empty;

        public int MethodValue
        {
            get { return (int)Method; }
        }
    }
}
