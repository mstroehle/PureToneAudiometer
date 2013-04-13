namespace PureToneAudiometer.ViewModels.Results
{
    using System;
    using Caliburn.Micro;

    public class ResultFileViewModel
    {
        public string FileName { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastChangedDate { get; set; }

        private readonly IEventAggregator eventAggregator;

        public ResultFileViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        public void Tapped()
        {
            eventAggregator.Publish(new Events.ResultItemTapped(FileName));
        }
    }
}
