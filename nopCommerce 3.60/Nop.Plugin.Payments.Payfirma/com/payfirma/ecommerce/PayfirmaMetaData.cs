using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.payfirma.ecommerce
{
    public class PayfirmaMetaData
    {
        /// <summary>
        /// Customer’s email address. Email addresses are used to link the transaction to the customer.
        /// If the customer’s email does not exist within My Customers during lookup, a new customer is created.
        /// By default PayHQ will send a receipt to the customer’s provided email address.
        /// </summary>
        public String Email { get; set; }

        /// <summary>
        /// The customer’s first name.
        /// </summary>
        public String Firstname { get; set; }

        /// <summary>
        /// The customer’s last name.
        /// </summary>
        public String Lastname { get; set; }

        /// <summary>
        /// Customer’s primary address line.
        /// </summary>
        public String Address1 { get; set; }

        /// <summary>
        /// Secondary address line.
        /// </summary>
        public String Address2 { get; set; }

        /// <summary>
        /// Customer’s city.
        /// </summary>
        public String City { get; set; }

        /// <summary>
        /// Customer’s province.
        /// </summary>
        public String Province { get; set; }

        /// <summary>
        /// Customer’s country.
        /// </summary>
        public String Country { get; set; }

        /// <summary>
        /// Customer’s postal code.
        /// </summary>
        public String PostalCode { get; set; }

        /// <summary>
        /// Customer’s company or business name.
        /// </summary>
        public String Company { get; set; }

        /// <summary>
        /// Customer’s phone number.
        /// </summary>
        public String Telephone { get; set; }

        /// <summary>
        /// Description of transaction.
        /// </summary>
        public String Description { get; set; }

        /// <summary>
        /// Order identifier.
        /// </summary>
        public String OrderId { get; set; }

        /// <summary>
        /// Invoice identifier.
        /// </summary>
        public String InvoiceId { get; set; }

        /// <summary>
        /// Used to indicate the dollar amount of any taxes applied to transactions.
        /// </summary>
        public String AmountTax { get; set; }

        /// <summary>
        /// Used to indicate the dollar amount of any tips applied to transactions.
        /// </summary>
        public String AmountTip { get; set; }

        /// <summary>
        /// When using more than one Gateway, it is possible to alternate between CAD and USD using the values "CA$" and "US$"
        /// </summary>
        public String Currency { get; set; }

        /// <summary>
        /// Used to send blind carbon email copies. Will only work if a primary email is selected
        /// </summary>
        public String BCCEmails { get; set; }
    }
}
