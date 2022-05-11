using System;

namespace FilePickerDemo.Models
{
    public class ErrorViewModel
    {
        public ErrorViewModel(string requestId)
        {
            RequestId = requestId;
        }

        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
