namespace FilesFunction
{
    internal class Constants
    {
        internal const string FilesSubscriptionServiceClientState = "GraphTutorialState";
        internal const string OneDriveFileNotificationsQueue = "onedrivefiles";
        internal static string[] BasePermissionScopes = new string[] { "user.read", "files.readwrite.all" };
    }
}
