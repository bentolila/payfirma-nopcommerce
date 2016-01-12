using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.payfirma.ecommerce
{
    public class PayfirmaTransactionResponse
    {
        public PayfirmaTransactionResponse() { }

        public String Error { get; set; }
        public String Type { get; set; }
        public String ResultMessage { get; set; }
        public Boolean Result { get; set; }
        public String CardType { get; set; }
        public String Amount { get; set; }
        public String TransactionId { get; set; }
        public String Suffix { get; set; }
        public String AVS { get; set; }
        public String CVV2 { get; set; }
        public String AuthCode { get; set; }
        public String Email { get; set; }
        public String Firstname { get; set; }
        public String Lastname { get; set; }
        public String Address1 { get; set; }
        public String Address2 { get; set; }
        public String City { get; set; }
        public String Province { get; set; }
        public String Country { get; set; }
        public String PostalCode { get; set; }
        public String Company { get; set; }
        public String Telephone { get; set; }
        public String Description { get; set; }
        public String OrderId { get; set; }
        public String InvoiceId { get; set; }
        public String CustomId { get; set; }

        public override string ToString()
        {
            return String.Join(Environment.NewLine,
                "Error: " + this.Error,
                "Type: " + this.Type,
                "Result Message: " + this.ResultMessage,
                "Result: " + this.Result.ToString(),
                "Card Type: " + this.CardType,
                "Amount: " + this.Amount,
                "Transaction ID: " + this.TransactionId,
                "Suffix: " + this.Suffix,
                "AVS: " + this.AVS,
                "CVV2: " + this.CVV2,
                "AuthCode: " + this.AuthCode,
                "Email: " + this.Email,
                "First Name: " + this.Firstname,
                "Last Name: " + this.Lastname,
                "Address 1: " + this.Address1,
                "Address 2: " + this.Address2,
                "City: " + this.City,
                "Province: " + this.Province,
                "Country: " + this.Country,
                "Postal Code: " + this.PostalCode,
                "Company: " + this.Company,
                "Telephone: " + this.Telephone,
                "Description: " + this.Description,
                "Order ID: " + this.OrderId,
                "Invoice ID: " + this.InvoiceId,
                "Custom ID: " + this.CustomId);
        }
    }
}
