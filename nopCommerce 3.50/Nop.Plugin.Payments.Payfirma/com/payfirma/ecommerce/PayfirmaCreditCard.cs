using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.payfirma.ecommerce
{
    public class PayfirmaCreditCard
    {
        public PayfirmaCreditCard() { }

        public String Number { get; set; }
        public int ExpMonth { get; set; }
        public int ExpYear { get; set; }
        public String CVV2 { get; set; }
    }
}
