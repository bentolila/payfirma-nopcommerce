using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace com.payfirma.ecommerce
{
    public class PayfirmaBase
    {
        public PayfirmaBase() { }

        protected PayfirmaTransactionResponse ProcessTransaction(String type, PayfirmaCredentials credentials, PayfirmaCreditCard cc, PayfirmaMetaData metaData, Double amount, Boolean isTest)
        {
            PayfirmaTransactionResponse payfirmaResponse = new PayfirmaTransactionResponse();
            var webClient = new WebClient();

            NameValueCollection requestData = new NameValueCollection();
            this.GenerateMetaData(metaData, requestData);
            requestData.Add("key", credentials.APIKey);
            requestData.Add("merchant_id", credentials.MerchantID);
            requestData.Add("amount", amount.ToString());
            requestData.Add("card_number", cc.Number);
            requestData.Add("card_expiry_month", cc.ExpMonth.ToString());
            requestData.Add("card_expiry_year", cc.ExpYear.ToString());
            requestData.Add("cvv2", cc.CVV2);
            if (isTest) { requestData.Add("test_mode", "true"); }

            requestData.Add("do_not_store", "true");

            if (type.ToLower() != "sale" && type.ToLower() != "authorize")
            {
                // Default to sale transaction type.
                type = "sale";
            }
            String url = this.GetPayfirmaURL() + "/" + type.ToLower(); ;

            try
            {
                var webResponseByte = webClient.UploadValues(url, "POST", requestData);
                var webResponse = Encoding.ASCII.GetString(webResponseByte);

                if (!String.IsNullOrEmpty(webResponse))
                {
                    payfirmaResponse = this.HandleTransacitonResponse(webResponse);
                }
                else
                {
                    payfirmaResponse.Error = "Payfirma unknown error";
                }
            }
            catch (Exception e)
            {
                payfirmaResponse.Error = "Payfirma Error: " + e.Message;
            }

            return payfirmaResponse;
        }

        protected PayfirmaTransactionResponse ProcessTransaction(String type, PayfirmaCredentials credentials, Double amount, String transactionId, Boolean isTest)
        {
            PayfirmaTransactionResponse payfirmaResponse = new PayfirmaTransactionResponse();
            var webClient = new WebClient();

            NameValueCollection requestData = new NameValueCollection();
            requestData.Add("key", credentials.APIKey);
            requestData.Add("merchant_id", credentials.MerchantID);
            requestData.Add("amount", amount.ToString());
            if (isTest) { requestData.Add("test_mode", "true"); }

            if (type.ToLower() != "refund" && type.ToLower() != "capture")
            {
                payfirmaResponse.Error = "Payfirma Invalid Transaction Type : " + type;
                return payfirmaResponse;
            }

            if (String.IsNullOrEmpty(transactionId))
            {
                payfirmaResponse.Error = "Payfirma Invalid Transaction ID";
                return payfirmaResponse;
            }

            String url = this.GetPayfirmaURL() + "/" + type.ToLower() + "/" + transactionId;

            try
            {
                var webResponseByte = webClient.UploadValues(url, "POST", requestData);
                var webResponse = Encoding.ASCII.GetString(webResponseByte);

                if (!String.IsNullOrEmpty(webResponse))
                {
                    payfirmaResponse = this.HandleTransacitonResponse(webResponse);
                }
                else
                {
                    payfirmaResponse.Error = "Payfirma unknown error";
                }
            }
            catch (Exception e)
            {
                payfirmaResponse.Error = "Payfirma Error: " + e.Message;
            }

            return payfirmaResponse;
        }

        protected String GetPayfirmaURL()
        {
            return "https://ecom.payfirma.com";
        }

        protected NameValueCollection GenerateMetaData(PayfirmaMetaData metaData, NameValueCollection data)
        {
            if (data == null) { data = new NameValueCollection(); }

            if (!String.IsNullOrEmpty(metaData.Firstname)) { data.Add("first_name", metaData.Firstname); }
            if (!String.IsNullOrEmpty(metaData.Lastname)) { data.Add("last_name", metaData.Lastname); }
            if (!String.IsNullOrEmpty(metaData.Company)) { data.Add("company", metaData.Company); }
            if (!String.IsNullOrEmpty(metaData.Address1)) { data.Add("address1", metaData.Address1); }
            if (!String.IsNullOrEmpty(metaData.Address2)) { data.Add("address2", metaData.Address2); }
            if (!String.IsNullOrEmpty(metaData.City)) { data.Add("city", metaData.City); }
            if (!String.IsNullOrEmpty(metaData.Province)) { data.Add("province", metaData.Province); }
            if (!String.IsNullOrEmpty(metaData.PostalCode)) { data.Add("postal_code", metaData.PostalCode); }
            if (!String.IsNullOrEmpty(metaData.Telephone)) { data.Add("telephone", metaData.Telephone); }
            if (!String.IsNullOrEmpty(metaData.Country)) { data.Add("country", metaData.Country); }
            if (!String.IsNullOrEmpty(metaData.Email)) { data.Add("email", metaData.Email); }
            if (!String.IsNullOrEmpty(metaData.Currency))
            {
                if (metaData.Currency == "CA$" || metaData.Currency == "US$")
                {
                    data.Add("currency", metaData.Currency);
                }
            }
            if (!String.IsNullOrEmpty(metaData.AmountTax)) { data.Add("amount_tax", metaData.AmountTax); }
            if (!String.IsNullOrEmpty(metaData.AmountTip)) { data.Add("amount_tip", metaData.AmountTip); }
            if (!String.IsNullOrEmpty(metaData.Description)) { data.Add("description", metaData.Description); }
            if (!String.IsNullOrEmpty(metaData.OrderId)) { data.Add("order_id", metaData.OrderId); }
            if (!String.IsNullOrEmpty(metaData.InvoiceId)) { data.Add("invoice_id", metaData.InvoiceId); }
            if (!String.IsNullOrEmpty(metaData.BCCEmails)) { data.Add("bcc_emails", metaData.BCCEmails); }
            return data;
        }

        protected PayfirmaTransactionResponse HandleTransacitonResponse(String webResponse)
        {
            PayfirmaTransactionResponse payfirmaResponse = new PayfirmaTransactionResponse();

            var responseObj = JsonConvert.DeserializeObject<dynamic>(webResponse);
            if (webResponse.Contains("\"error\""))
            {
                // Transaction Failed                    
                payfirmaResponse.Error = responseObj.error;
            }
            else
            {
                payfirmaResponse.Type = responseObj.type;
                payfirmaResponse.ResultMessage = responseObj.result;
                payfirmaResponse.Result = responseObj.result_bool;
                payfirmaResponse.CardType = responseObj.card_type;
                payfirmaResponse.Amount = responseObj.amount;
                payfirmaResponse.TransactionId = responseObj.transaction_id;
                payfirmaResponse.Suffix = responseObj.suffix;
                payfirmaResponse.AVS = responseObj.avs;
                payfirmaResponse.CVV2 = responseObj.cvv2;
                payfirmaResponse.AuthCode = responseObj.auth_code;
                payfirmaResponse.Email = responseObj.email;
                payfirmaResponse.Firstname = responseObj.first_name;
                payfirmaResponse.Lastname = responseObj.last_name;
                payfirmaResponse.Address1 = responseObj.address1;
                payfirmaResponse.Address2 = responseObj.address2;
                payfirmaResponse.City = responseObj.city;
                payfirmaResponse.Province = responseObj.province;
                payfirmaResponse.Country = responseObj.country;
                payfirmaResponse.PostalCode = responseObj.postal_code;
                payfirmaResponse.Company = responseObj.company;
                payfirmaResponse.Telephone = responseObj.telephone;
                payfirmaResponse.Description = responseObj.description;
                payfirmaResponse.OrderId = responseObj.order_id;
                payfirmaResponse.InvoiceId = responseObj.invoice_id;
                payfirmaResponse.CustomId = responseObj.custom_id;
            }

            return payfirmaResponse;
        }
    }
}
