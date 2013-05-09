namespace PureToneAudiometer.ViewModels.Results
{
    using System;

    public class ResultFileViewModel
    {
        public string FileName { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastChangedDate { get; set; }
    }
}
