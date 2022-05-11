namespace FilesFunction
{
    /// <summary>
    /// List of change notifications: a subscription service can receive changes from multiple services
    /// </summary>
    public class NotificationList
    {
        public ChangeNotification[] Value { get; set; }
    }
}
