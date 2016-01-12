using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.Payfirma
{
    public class PayfirmaPaymentSettings : ISettings
    {
        public String APIKey { get; set; }
        public String MerchantId { get; set; }
        public TransactMode TransactMode { get; set; }
        public bool IsTest { get; set; }
    }
}
