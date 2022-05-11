using System.Collections.Generic;

namespace FilePickerDemo.Models.File
{
    /// <summary>
    /// Authentication setup
    /// </summary>
    public class AuthenticationConfiguration
    {
        /// <summary>
        /// Configuration for auth-specific messaging.
        /// If the File Browser must obtain authentication from a different source than its parent window,
        /// the initializing window should pass that information here.
        /// The File Browser will ask its parent for a `window` with which it will set up communication,
        /// but that `window` must be able to receive messages sent to the `origin` specified here.
        /// </summary>
        public MessagingAuthenticationConfiguration? Messaging { get; set; }

        /// <summary>
        /// Array of types for which the Host app can provide tokens.
        /// </summary>
        public List<AuthenticationProvider>? Providers { get; set; }

        /// <summary>
        /// Array of tokens which may be used for initial data requests.
        /// Minimally, the File Browser will need a token for the Web origin against which the ASPX page is loaded.
        /// </summary>
        public List<TokenAuthenticationConfiguration>? Tokens { get; set; }
    }

    /// <summary>
    /// Configuration for auth-specific messaging.
    /// If the File Browser must obtain authentication from a different source than its parent window,
    /// the initializing window should pass that information here.
    /// The File Browser will ask its parent for a `window` with which it will set up communication,
    /// but that `window` must be able to receive messages sent to the `origin` specified here.
    /// </summary>
    public class MessagingAuthenticationConfiguration
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="origin">The host app's authority</param>
        /// <param name="channelId">A unique id assigned by the host app to this File Browser instance</param>
        public MessagingAuthenticationConfiguration(string origin, string channelId)
        {
            Origin = origin;
            ChannelId = channelId;
        }

        /// <summary>
        /// The host app's authority, used as the target origin for post-messaging.
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// A unique id assigned by the host app to this File Browser instance.
        /// </summary>
        public string ChannelId { get; set; }
    }

    /// <summary>
    /// Supported type of tokens
    /// </summary>
    public enum AuthenticationProvider
    {
        /// <summary>
        /// SharePoint token
        /// </summary>
        sharePoint = 0,

        /// <summary>
        /// Microsoft Graph token
        /// </summary>
        graph = 1
    }

    /// <summary>
    /// Configuration of a passed in token
    /// </summary>
    public class TokenAuthenticationConfiguration
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="resource">Target audience for the token</param>
        public TokenAuthenticationConfiguration(string resource)
        {
            Resource = resource;
        }

        /// <summary>
        /// Type of the token
        /// </summary>
        public AuthenticationProvider Type { get; set; }

        /// <summary>
        /// Target audience for the token.
        /// For a SharePoint token, the domain of the SharePoint content.
        /// For a Graph token, the domain of the Graph endpoint.
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// Timestamp for token expiration
        /// </summary>
        public int? Expires { get; set; }
    }

    /// <summary>
    /// Configuration of the messaging between host and control (iframe)
    /// </summary>
    public class MessagingConfiguration
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="channelId">A unique id assigned by the host app to this File Browser instance</param>
        /// <param name="origin">The host app's authority</param>
        public MessagingConfiguration(string channelId, string origin)
        {
            ChannelId = channelId;
            Origin = origin;
        }

        /// <summary>
        /// A unique id assigned by the host app to this File Browser instance.
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// The host app's authority, used as the target origin for post-messaging.
        /// </summary>
        public string Origin { get; set; }
    }

    /// <summary>
    /// Configuration for the entry location to which the File Browser will navigate on load.
    /// The File Browser app will prioritize path-based navigation if provided, falling back to other address forms
    /// on error(in case of Site redirection or content rename) or if path information is not provided.
    /// </summary>
    public class EntryConfiguration
    {
        /// <summary>
        /// Specify an exact SharePoint content location
        /// </summary>
        public SharePointEntryConfiguration? SharePoint { get; set; }

        /// <summary>
        /// Specify an exact OneDrive content location
        /// </summary>
        public OneDriveEntryConfiguration? OneDrive { get; set; }
    }

    /// <summary>
    /// Specify an exact SharePoint content location
    /// </summary>
    public class SharePointEntryConfiguration
    {
        /// <summary>
        /// Specify an exact SharePoint content location by path segments.
        /// </summary>
        public SharePointEntryConfigurationByPath? ByPath { get; set; }

        /// <summary>
        /// Specify an exact SharePoint content location by id's
        /// </summary>
        public SharePointEntryConfigurationById? ById { get; set; }
    }

    /// <summary>
    /// Specify an exact SharePoint content location by path segments.
    /// </summary>
    public class SharePointEntryConfigurationByPath
    {
        /// <summary>
        /// Full URL to the root of a Web, or server-relative URL.
        /// example: 'https://contoso-my.sharepoint.com/personal/user_contoso_com'
        /// example: '/path/to/web'
        /// example: 'subweb'
        /// </summary>
        public string? Web { get; set; }

        /// <summary>
        /// Full URL or path segement to identity a List.
        /// If not preceded with a `/` or a URL scheme, this is assumed to be a list in the specified web.
        /// example: 'Shared Documents'
        /// example: '/path/to/web/Shared Documents'
        /// example: 'https://contoso.sharepoint.com/path/to/web/Shared Documents'
        /// </summary>
        public string? List { get; set; }

        /// <summary>
        /// Path segment to a folder within a list, or a server-relative URL to a folder.
        /// example: 'General'
        /// example: 'foo/bar'
        /// example: '/path/to/web/Shared Documents/General'
        /// </summary>
        public string? Folder { get; set; }
    }

    /// <summary>
    /// Indicates SharePoint ID values which may be used as a backup in case path-based navigation to the initial item fails.
    /// Id-based lookup in SharePoint is slower, as should only be used as a last-resort.
    /// The File Browser will return an error if only ID values are specified.
    /// </summary>
    public class SharePointEntryConfigurationById
    {
        /// <summary>
        /// Id of the web to use
        /// </summary>
        public string? WebId { get; set; }

        /// <summary>
        /// Id of the list to use
        /// </summary>
        public string? ListId { get; set; }

        /// <summary>
        /// Unique id of the item to use
        /// </summary>
        public string? UniqueId { get; set; }
    }

    /// <summary>
    /// Specify an exact OneDrive content location
    /// </summary>
    public class OneDriveEntryConfiguration
    {
        /// <summary>
        /// Specify an exact OneDrive content location by folder
        /// </summary>
        public FilesOneDriveEntryConfiguration? Files { get; set; }
    }

    /// <summary>
    /// Specify an exact OneDrive content location by folder.
    /// </summary>
    public class FilesOneDriveEntryConfiguration
    {
        /// <summary>
        /// Path segment to a folder within a list, or a server-relative URL to a folder.
        /// </summary>
        public string? Folder { get; set; }
    }

    /// <summary>
    /// Configuration for the control localization
    /// </summary>
    public class LocalizationConfiguration
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="language">The language code from the Host application</param>
        public LocalizationConfiguration(string language)
        {
            Language = language;
        }

        /// <summary>
        /// The language code from the Host application.
        /// File Browser will render components which are not user content using the specified language.
        /// If the backing SharePoint Web has an override language setting, some strings such as column headers will render
        /// using the Web's language instead.
        /// </summary>
        public string Language { get; set; }
    }

    /// <summary>
    /// Custom theme to apply
    /// </summary>
    public class Theme
    {
        /// <summary>
        /// Custom theme palette
        /// </summary>
        public Palette? Palette { get; set; }
    }
    
    /// <summary>
    /// Defines a custom theme palette
    /// </summary>
    public class Palette
    {
        public string? ThemePrimary { get; set; }
        public string? ThemeLighterAlt { get; set; }
        public string? ThemeLighter { get; set; }
        public string? ThemeLight { get; set; }
        public string? ThemeTertiary { get; set; }
        public string? ThemeSecondary { get; set; }
        public string? ThemeDarkAlt { get; set; }
        public string? ThemeDark { get; set; }
        public string? ThemeDarker { get; set; }
        public string? NeutralLighterAlt { get; set; }
        public string? NeutralLighter { get; set; }
        public string? NeutralLight { get; set; }
        public string? NeutralQuaternaryAlt { get; set; }
        public string? NeutralQuaternary { get; set; }
        public string? NeutralTertiaryAlt { get; set; }
        public string? NeutralTertiary { get; set; }
        public string? NeutralSecondary { get; set; }
        public string? NeutralPrimaryAlt { get; set; }
        public string? NeutralPrimary { get; set; }
        public string? NeutralDark { get; set; }
        public string? Black { get; set; }
        public string? White { get; set; }
    }

}
