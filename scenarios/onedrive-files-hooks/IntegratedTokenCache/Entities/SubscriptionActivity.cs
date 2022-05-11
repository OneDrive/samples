namespace IntegratedTokenCache.Entities
{
    /// <summary>
    /// This entity represents subscription information, needed to work with subscriptions. Feel free to extend if needed
    /// </summary>
    public class SubscriptionActivity
    {
        public SubscriptionActivity(string accountObjectId, string accountTenantId, string userPrincipalName)
        {
            AccountObjectId = accountObjectId;
            AccountTenantId = accountTenantId;
            UserPrincipalName = userPrincipalName;
        }

        public string AccountObjectId { get; set; }
        
        public string AccountTenantId { get; set; }
        
        public string UserPrincipalName { get; set; }
        
        public string SubscriptionId { get; set; }
        
        public string LastChangeToken { get; set; }
    }
}
