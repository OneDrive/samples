using System;

namespace FilesFunction
{
    /// <summary>
    /// Change event (sent by OneDrive to the subscription service)
    /// </summary>
    public class ChangeNotification
    {
        public string ChangeType { get; set; }
        public string ClientState { get; set; }
        public string Resource { get; set; }
        public ResourceData ResourceData { get; set; }
        public DateTime SubscriptionExpirationDateTime { get; set; }
        public string SubscriptionId { get; set; }
        public string TenantId { get; set; }
    }
}
