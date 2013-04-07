namespace PureToneAudiometer.Views.Core
{
    using System;
    using Audio;
    using Caliburn.Micro;

    public partial class HearingTestView :  IHandle<Events.HearingTest.PitchGeneratorChanged>, 
                                            IHandle<Events.HearingTest.ChannelChanged>, 
                                            IHandle<Events.HearingTest.StartPlaying>,
                                            IHandle<Events.HearingTest.StopPlaying>,
                                            IHandle<Events.HearingTest.Deactivate>
    {
        private IPitchGenerator pitchGenerator;
        private readonly IEventAggregator eventAggregator;

        public HearingTestView(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.eventAggregator.Subscribe(this);
            InitializeComponent();
        }

        public void Handle(Events.HearingTest.PitchGeneratorChanged message)
        {
            pitchGenerator = message.PitchGenerator;
            Media.SetSource(new PureToneSource(pitchGenerator, TimeSpan.FromSeconds(1),
                                               new PauseDuration(TimeSpan.FromMilliseconds(100),
                                                                 TimeSpan.FromMilliseconds(1200))));
        }

        public void Handle(Events.HearingTest.ChannelChanged message)
        {
            switch (message.NewActiveChannel)
            {
                case Channel.Right:
                    Media.Balance = 1;
                    break;
                case Channel.Left:
                    Media.Balance = -1;
                    break;
                default:
                    Media.Balance = 0;
                    break;
            }
        }

        public void Handle(Events.HearingTest.StartPlaying message)
        {
            Media.Stop();
            Media.SetSource(new PureToneSource(pitchGenerator, TimeSpan.FromMilliseconds(message.Preset.PitchDuration),
                                               new PauseDuration(
                                                   TimeSpan.FromMilliseconds(message.Preset.MinimumPauseDuration),
                                                   TimeSpan.FromMilliseconds(message.Preset.MaximumPauseDuration))));
            Media.Play();
        }

        public void Handle(Events.HearingTest.StopPlaying message)
        {
            Media.Stop();
        }

        public void Handle(Events.HearingTest.Deactivate message)
        {
            Media.Stop();
            eventAggregator.Unsubscribe(this);
        }
    }
}