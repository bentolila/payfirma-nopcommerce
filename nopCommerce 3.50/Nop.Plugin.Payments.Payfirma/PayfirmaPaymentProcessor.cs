using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Text;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.Payfirma.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using com.payfirma.ecommerce;


namespace Nop.Plugin.Payments.Payfirma
{
    /// <summary>
    /// Payfirma Payment Processor
    /// </summary>
    public class PayfirmaPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly PayfirmaPaymentSettings _payfirmaPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IEncryptionService _encryptionService;

        #endregion

        #region Ctor

        public PayfirmaPaymentProcessor(PayfirmaPaymentSettings payfirmaPaymentSettings,
            ISettingService settingService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            CurrencySettings currencySettings, IWebHelper webHelper,
            IOrderTotalCalculationService orderTotalCalculationService, IEncryptionService encryptionService)
        {
            this._payfirmaPaymentSettings = payfirmaPaymentSettings;
            this._settingService = settingService;
            this._currencyService = currencyService;
            this._customerService = customerService;
            this._currencySettings = currencySettings;
            this._webHelper = webHelper;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._encryptionService = encryptionService;
        }

        #endregion

        #region Utilities

        private PayfirmaCredentials PopulateMerchantCredentials()
        {
            var credentails = new PayfirmaCredentials()
            {
                APIKey = _payfirmaPaymentSettings.APIKey,
                MerchantID = _payfirmaPaymentSettings.MerchantId
            };
            return credentails;
        }

        #endregion

        #region Methods

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();

            var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);

            // Credit Card Info
            PayfirmaCreditCard cc = new PayfirmaCreditCard()
            {
                Number = processPaymentRequest.CreditCardNumber,
                ExpMonth = processPaymentRequest.CreditCardExpireMonth,
                ExpYear = processPaymentRequest.CreditCardExpireYear,
                CVV2 = processPaymentRequest.CreditCardCvv2
            };

            // Extra Meta Data
            PayfirmaMetaData payfirmaMeta = new PayfirmaMetaData();
            payfirmaMeta.Firstname = customer.BillingAddress.FirstName;
            payfirmaMeta.Lastname = customer.BillingAddress.LastName;
            if (!String.IsNullOrEmpty(customer.BillingAddress.Company)) { payfirmaMeta.Company = customer.BillingAddress.Company; }
            payfirmaMeta.Address1 = customer.BillingAddress.Address1;
            if (!String.IsNullOrEmpty(customer.BillingAddress.Address2)) { payfirmaMeta.Address2 = customer.BillingAddress.Address2; }
            payfirmaMeta.City = customer.BillingAddress.City;
            if (customer.BillingAddress.StateProvince != null)
            {
                payfirmaMeta.Province = customer.BillingAddress.StateProvince.Name;
            }
            payfirmaMeta.PostalCode = customer.BillingAddress.ZipPostalCode;
            if (customer.BillingAddress.Country != null)
            {
                payfirmaMeta.Country = customer.BillingAddress.Country.Name;
            }
            payfirmaMeta.Email = customer.BillingAddress.Email;

            String currencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            payfirmaMeta.Currency = "CA$";
            if (currencyCode == "USD") { payfirmaMeta.Currency = "US$"; }
            payfirmaMeta.OrderId = processPaymentRequest.OrderGuid.ToString();
            if (!String.IsNullOrEmpty(customer.BillingAddress.PhoneNumber)) { payfirmaMeta.Telephone = customer.BillingAddress.PhoneNumber;  }
            payfirmaMeta.Description = "Payment via nopCommerce.";

            PayfirmaTransaction payfirma = new PayfirmaTransaction();
            PayfirmaTransactionResponse payfirmaResponse;
            if (_payfirmaPaymentSettings.TransactMode == TransactMode.Authorize)
            {
                payfirmaResponse = payfirma.ProcessAuthorize(this.PopulateMerchantCredentials(), cc, payfirmaMeta,
                    Convert.ToDouble(processPaymentRequest.OrderTotal), _payfirmaPaymentSettings.IsTest);
            }
            else
            {
                payfirmaResponse = payfirma.ProcessSale(this.PopulateMerchantCredentials(), cc, payfirmaMeta,
                   Convert.ToDouble(processPaymentRequest.OrderTotal), _payfirmaPaymentSettings.IsTest);
            }

            if (!String.IsNullOrEmpty(payfirmaResponse.Error))
            {
                result.AddError(payfirmaResponse.Error);
            }
            else if (!payfirmaResponse.Result)
            {
                result.AddError(payfirmaResponse.ResultMessage);
            } else {
                result.AvsResult = payfirmaResponse.AVS;
                result.AuthorizationTransactionCode = payfirmaResponse.AuthCode;
                result.AuthorizationTransactionId = payfirmaResponse.TransactionId;
                result.AuthorizationTransactionResult = payfirmaResponse.ResultMessage;

                if (_payfirmaPaymentSettings.TransactMode == TransactMode.Authorize)
                {
                    result.NewPaymentStatus = PaymentStatus.Authorized;
                }
                else
                {
                    result.NewPaymentStatus = PaymentStatus.Paid;
                }
            }

            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            // Do Nothing.
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return 0;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();

            PayfirmaTransaction payfirma = new PayfirmaTransaction();
            PayfirmaTransactionResponse payfirmaResponse =
                payfirma.ProcessCapture(this.PopulateMerchantCredentials(), capturePaymentRequest.Order.AuthorizationTransactionId,
                Convert.ToDouble(capturePaymentRequest.Order.OrderTotal), _payfirmaPaymentSettings.IsTest);

            if (!String.IsNullOrEmpty(payfirmaResponse.Error))
            {
                result.AddError(payfirmaResponse.Error);
            }
            else if (!payfirmaResponse.Result)
            {
                result.AddError(payfirmaResponse.ResultMessage);
            }
            else
            {
                result.CaptureTransactionId = payfirmaResponse.TransactionId;
                result.CaptureTransactionResult = payfirmaResponse.ResultMessage;
                result.NewPaymentStatus = PaymentStatus.Paid;
            }

            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            PayfirmaTransaction payfirma = new PayfirmaTransaction();
            PayfirmaTransactionResponse payfirmaResponse =
                payfirma.ProcessRefund(this.PopulateMerchantCredentials(), refundPaymentRequest.Order.AuthorizationTransactionId,
                Convert.ToDouble(refundPaymentRequest.AmountToRefund), _payfirmaPaymentSettings.IsTest);

            if (!String.IsNullOrEmpty(payfirmaResponse.Error))
            {
                result.AddError(payfirmaResponse.Error);
            }
            else if (!payfirmaResponse.Result)
            {
                result.AddError(payfirmaResponse.ResultMessage);
            }
            else
            {
                var isOrderFullyRefunded = (refundPaymentRequest.AmountToRefund + refundPaymentRequest.Order.RefundedAmount == refundPaymentRequest.Order.OrderTotal);
                result.NewPaymentStatus = isOrderFullyRefunded ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded;
            }

            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            throw new NotSupportedException();
        }

        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //it's not a redirection payment method. So we always return false
            return false;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentPayfirma";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.Payfirma.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentPayfirma";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.Payfirma.Controllers" }, { "area", null } };
        }

        public override void Install()
        {
            //settings
            var settings = new PayfirmaPaymentSettings
            {
                APIKey = "YourAPIKey",
                MerchantId = "YourMerchantId",
                TransactMode = Payfirma.TransactMode.AuthorizeAndCapture,
                IsTest = true
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Payfirma.Notes", "If you're using this gateway, ensure that your primary store currency is either CAD or USD.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Payfirma.Fields.IsTest", "Set Payfirma in Test Mode");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Payfirma.Fields.IsTest.Hint", "Check to enable Test Mode (testing environment).");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Payfirma.Fields.TransactModeValues", "Transaction mode");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Payfirma.Fields.TransactModeValues.Hint", "Choose transaction mode");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Payfirma.Fields.MerchantId", "Merchant ID");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Payfirma.Fields.MerchantId.Hint", "Specify Merchant ID");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Payfirma.Fields.APIKey", "API Key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Payfirma.Fields.APIKey.Hint", "Specify API Key.");

            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<PayfirmaPaymentSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.Payfirma.Notes");
            this.DeletePluginLocaleResource("Plugins.Payments.Payfirma.Fields.IsTest");
            this.DeletePluginLocaleResource("Plugins.Payments.Payfirma.Fields.IsTest.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Payfirma.Fields.TransactModeValues");
            this.DeletePluginLocaleResource("Plugins.Payments.Payfirma.Fields.TransactModeValues.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Payfirma.Fields.MerchantId");
            this.DeletePluginLocaleResource("Plugins.Payments.Payfirma.Fields.MerchantId.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Payfirma.Fields.APIKey");
            this.DeletePluginLocaleResource("Plugins.Payments.Payfirma.Fields.APIKey.Hint");

            base.Uninstall();
        }
        #endregion

        #region Properties

        public Type GetControllerType()
        {
            return typeof(PaymentPayfirmaController);        
        }

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method. TODO: Implement the Recurring Payment to Payfirma
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Standard;
            }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get
            {
                return false;
            }
        }

        #endregion
    }
}
