using Microsoft.Identity.Client;

namespace FilesFunction.Model
{
    /// <summary>
    /// Msal expects an object of IAccount in the method AcquireTokenSilent. This class implements the IAccount interface and will be instantiated 
    /// with the values that came from MsalAccountActivity entity, and will be sent as parameter to AcquireTokenSilent, so Msal can grab the 
    /// cached token for that particular user account.
    /// </summary>
    public class MsalAccount : IAccount
    {
        public MsalAccount() { }

        public MsalAccount(string objectId, string tenantId)
        {
            HomeAccountId = new AccountId($"{objectId}.{tenantId}", objectId, tenantId);
        }

        public string Username { get; set; }

        public string Environment { get; set; }

        public AccountId HomeAccountId { get; set; }
    }
}
