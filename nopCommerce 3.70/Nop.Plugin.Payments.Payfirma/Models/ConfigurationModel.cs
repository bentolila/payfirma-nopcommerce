using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.Payfirma.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Payfirma.Fields.APIKey")]
        public string APIKey { get; set; }
        public bool APIKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Payfirma.Fields.MerchantId")]
        public string MerchantId { get; set; }
        public bool MerchantId_OverrideForStore { get; set; }

        public int TransactModeId { get; set; }
        public bool TransactModeId_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Payments.Payfirma.Fields.TransactModeValues")]
        public SelectList TransactModeValues { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Payfirma.Fields.IsTest")]
        public bool IsTest { get; set; }
        public bool IsTest_OverrideForStore { get; set; }
    }
}
