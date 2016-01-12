using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace com.payfirma.ecommerce
{
    /// <summary>
    /// Payfirma (payfirma.com) Transaction Class.
    /// 
    /// Author: Amram Bentolila
    /// </summary>
    public class PayfirmaTransaction : PayfirmaBase
    {

        public PayfirmaTransaction() {

        }

        public PayfirmaTransactionResponse ProcessSale(PayfirmaCredentials credentials, PayfirmaCreditCard cc, PayfirmaMetaData metaData, Double amount, Boolean isTest)
        {
            return base.ProcessTransaction("sale", credentials, cc, metaData, amount, isTest);
        }

        public PayfirmaTransactionResponse ProcessAuthorize(PayfirmaCredentials credentials, PayfirmaCreditCard cc, PayfirmaMetaData metaData, Double amount, Boolean isTest)
        {
            return base.ProcessTransaction("authorize", credentials, cc, metaData, amount, isTest);
        }

        public PayfirmaTransactionResponse ProcessCapture(PayfirmaCredentials credentials, String transactionId, Double amount, Boolean isTest)
        {
            return base.ProcessTransaction("capture", credentials, amount, transactionId, isTest);
        }

        public PayfirmaTransactionResponse ProcessRefund(PayfirmaCredentials credentials, String transactionId, Double amount, Boolean isTest)
        {
            return base.ProcessTransaction("refund", credentials, amount, transactionId, isTest);
        }
    }
}
