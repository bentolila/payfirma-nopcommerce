using Nop.Web.Framework;

namespace Nop.Plugin.Payments.Payfirma
{
    /// <summary>
    /// Represents Payfirma payment processor transaction mode
    /// </summary>
    public enum TransactMode
    {
        /// <summary>
        /// Authorize
        /// </summary>
        Authorize = 1,
        /// <summary>
        /// Authorize and capture
        /// </summary>
        AuthorizeAndCapture = 2
    }
}