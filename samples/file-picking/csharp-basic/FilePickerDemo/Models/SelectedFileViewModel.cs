namespace FilePickerDemo.Models
{
    public class SelectedFileViewModel
    {
        public SelectedFileViewModel()
        {

        }

        public string? AccessToken { get; set; }

        public string? FileId { get; set; }

        public string? DriveId { get; set; }

        public string? Thumbnail { get; set; }
    }
}
