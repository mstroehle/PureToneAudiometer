namespace PureToneAudiometer.ViewModels.Presets
{
    using System;
    using Caliburn.Micro;

    public class FileViewModel
    {
        public string PresetName { get; set; }
        public DateTime CreationDate { get; set; }

        private readonly IEventAggregator eventAggregator;

        public FileViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        public void ScheduleForDeletion()
        {
            eventAggregator.Publish(new Events.PresetScheduledForDeletion(PresetName));
        }
    }
}
