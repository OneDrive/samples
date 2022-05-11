using System.Collections.Generic;

namespace FilePickerDemo.Models.File
{
    /// <summary>
    /// File Picker configuration
    /// </summary>
    public class FilePickerOptions
    {
        /// <summary>
        /// SDK version
        /// </summary>
        public string Sdk { get; set; } = "v8.0";

        /// <summary>
        /// Messaging setup
        /// </summary>
        public MessagingConfiguration? Messaging { get; set; }

        /// <summary>
        /// File browser entry configuration: what to show
        /// </summary>
        public EntryConfiguration? Entry { get; set; }

        /// <summary>
        /// Authentication configuration
        /// </summary>
        public AuthenticationConfiguration? Authentication { get; set; }

        /// <summary>
        /// Localization configuration
        /// </summary>
        public LocalizationConfiguration? Localization { get; set; }

        /// <summary>
        /// Specified how many items may be picked
        /// </summary>
        public SelectionConfiguration? Selection { get; set; }

        /// <summary>
        /// Specifies what types of items may be picked and where they come from
        /// </summary>
        public TypesAndSourcesConfiguration? TypesAndSources { get; set; }

        /// <summary>
        ///  Specifies what happens when users pick files and what the user may do with files in the picker
        /// </summary>
        public PickerCommandConfiguration? Commands { get; set; }

        /// <summary>
        /// Specifies accessibility cues such as auto-focus behaviors
        /// </summary>
        public AccessibilityConfiguration? Accessibility { get; set; }

        /// <summary>
        /// Custom theme to apply to the control
        /// </summary>
        public Theme? Theme { get; set; }
    }

    /// <summary>
    /// Configuration for the file picker commands
    /// </summary>
    public class PickerCommandConfiguration
    {
        /// <summary>
        /// Sets the default 'pick' behavior once the user selects items
        /// </summary>
        public PickCommandConfiguration? Pick { get; set; }

        /// <summary>
        /// Sets the default 'close' behavior once the user selects items
        /// </summary>
        public CloseCommandConfiguration? Close { get; set; }
    }

    /// <summary>
    /// Configuration for the Pick command
    /// </summary>
    public class PickCommandConfiguration
    {
        /// <summary>
        /// A custom label to apply to the button to pick the items.
        /// This string must be localized if provided.
        /// </summary>
        public string? Label { get; set; }
    }

    /// <summary>
    /// Configuration for the Close command
    /// </summary>
    public class CloseCommandConfiguration
    {
        /// <summary>
        /// A custom label to apply to the button to close the picker.
        /// This string must be localized if provided.
        /// </summary>
        public string? Label { get; set; }
    }

    public class TypesAndSourcesConfiguration
    {
        /// <summary>
        /// Specifies the general category of items picked. Switches between 'file' vs. 'folder' picker mode,
        /// or a general-purpose picker. Default = all
        /// </summary>
        public SelectionType? Mode { get; set; }

        /// <summary>
        /// Filters to apply in the picker, limiting to shown files to certain extensions (e.g. ".pptx", ".jpg")
        /// </summary>
        public List<string>? Filters { get; set; }

        /// <summary>
        /// Configures whether or not specific pivots may be browsed for content by the user
        /// </summary>
        public SelectionPivots? Pivots { get; set; }
    }

    /// <summary>
    /// Pivots that are displayed by the picker
    /// </summary>
    public class SelectionPivots
    {
        /// <summary>
        /// Show the recent files pivot
        /// </summary>
        public bool? Recent { get; set; }

        /// <summary>
        /// Show the OneDrive files pivot
        /// </summary>
        public bool? OneDrive { get; set; }
    }

    /// <summary>
    /// Specifies the general category of items picked. Switches between 'file' vs. 'folder' picker mode,
    /// or a general-purpose picker. Default = all
    /// </summary>
    public enum SelectionType
    {
        /// <summary>
        /// Only allow files to be selected
        /// </summary>
        files,

        /// <summary>
        /// Only allow folders to be selected
        /// </summary>
        folders,

        /// <summary>
        /// Allow files and folders to be selected (default)
        /// </summary>
        all
    }

    /// <summary>
    /// Configuration for the file/folder selection behaviour
    /// </summary>
    public class SelectionConfiguration
    {
        /// <summary>
        /// Defines the file/folder selection behaviour
        /// </summary>
        public SelectionMode? Mode {get;set;}
    }

    /// <summary>
    /// Configuration for what item types may be selected within the picker and returned to the host
    /// </summary>
    public enum SelectionMode
    {
        /// <summary>
        /// Only select one (=default)
        /// </summary>
        single,
        /// <summary>
        /// Select multiple
        /// </summary>
        multiple,
        /// <summary>
        /// Pick one file by clicking on the file
        /// </summary>
        pick
    }

    /// <summary>
    /// Control accessibility configuration
    /// </summary>
    public class AccessibilityConfiguration
    {
        /// <summary>
        /// Focus trap configuration
        /// </summary>
        public FocusTrap? FocusTrap { get; set; }
    }

    /// <summary>
    /// Focus trap configuration
    /// </summary>
    public enum FocusTrap
    {
        /// <summary>
        /// During initial control load
        /// </summary>
        initial,

        /// <summary>
        /// Always
        /// </summary>
        always,

        /// <summary>
        /// No focus trap configuration
        /// </summary>
        none
    }

}
