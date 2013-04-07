namespace PureToneAudiometer.ViewModels.Core
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Audio;
    using Caliburn.Micro;
    using Presets;

    public class HearingTestViewModel : ViewModelBase
    {
        public string PresetFileName
        {
            get { return presetFileName; }
            set
            {
                if (value == presetFileName) return;
                presetFileName = value;
                NotifyOfPropertyChange(() => PresetFileName);
                NotifyOfPropertyChange(() => PresetName);
            }
        }

        public string PresetName
        {
            get { return Path.GetFileNameWithoutExtension(PresetFileName); }
        }

        private readonly IXmlItemsFileManager<PresetItemViewModel> itemsFileManager;
        private Queue<PresetItemViewModel> presetQueue;
        private IList<PresetItemViewModel> presetItems;
        private string currentFrequency;
        private string currentVolume;

        public string CurrentFrequency
        {
            get { return currentFrequency; }
            set
            {
                if (value == currentFrequency) return;
                currentFrequency = value;
                NotifyOfPropertyChange(() => CurrentFrequency);
            }
        }

        public string CurrentVolume
        {
            get { return currentVolume; }
            set
            {
                if (value == currentVolume) return;
                currentVolume = value;
                NotifyOfPropertyChange(() => CurrentVolume);
            }
        }

        public Channel CurrentChannel
        {
            get { return currentChannel; }
            set
            {
                if (value == currentChannel) return;
                currentChannel = value;
                NotifyOfPropertyChange(() => CurrentChannel);
            }
        }

        public string PlayContent
        {
            get { return playContent; }
            set
            {
                if (value == playContent) return;
                playContent = value;
                NotifyOfPropertyChange(() => PlayContent);
            }
        }
        public bool IsPlaying { get; set; }

        private readonly IEventAggregator eventAggregator;

        private Channel currentChannel;
        private string playContent;


        private readonly IPitchGenerator pitchGenerator;
        private PresetItemViewModel currentItem;
        private readonly IDictionary<string, object> settings;
        private int maxVolume;
        private string presetFileName;

        public HearingTestViewModel(INavigationService navigationService, 
            IXmlItemsFileManager<PresetItemViewModel> itemsFileManager, 
            IDictionary<string, object> settings,
            IEventAggregator eventAggregator, 
            IPitchGenerator pitchGenerator) : base(navigationService)
        {
            this.settings = settings;
            this.pitchGenerator = pitchGenerator;
            IsPlaying = true;
            PlayContent = IsPlaying ? "Pause" : "Play";
            CurrentChannel = Channel.None;
            this.eventAggregator = eventAggregator;
            presetItems = new List<PresetItemViewModel>(10);
            presetQueue = new Queue<PresetItemViewModel>(10);
            this.itemsFileManager = itemsFileManager;
        }

        public void Play()
        {
            IsPlaying = !IsPlaying;

            PlayContent = IsPlaying ? "Pause" : "Play";

            if(IsPlaying)
                eventAggregator.Publish(new Events.HearingTest.StartPlaying(currentItem));
            else
            {
                eventAggregator.Publish(new Events.HearingTest.StopPlaying());
            }
        }

        public void Ok()
        {
            eventAggregator.Publish(new Events.HearingTest.StopPlaying());

            if (presetQueue.Count > 0)
            {
                currentItem = presetQueue.Dequeue();
                eventAggregator.Publish(new Events.HearingTest.StartPlaying(currentItem));
            }
            else
            {
                CurrentChannel = CurrentChannel == Channel.Right ? Channel.Left : Channel.Right;
                eventAggregator.Publish(new Events.HearingTest.ChannelChanged(CurrentChannel));
                presetQueue = new Queue<PresetItemViewModel>(presetItems);
                currentItem = presetQueue.Dequeue();
            }

            pitchGenerator.Frequency = currentItem.Frequency;
            CurrentFrequency = currentItem.Frequency.ToString(CultureInfo.InvariantCulture);
            MuteVolume();
            eventAggregator.Publish(new Events.HearingTest.StartPlaying(currentItem));
        }

        private void ChangeVolume(int delta)
        {
            pitchGenerator.Attenuation = pitchGenerator.Attenuation + delta;
            CurrentVolume = (maxVolume + pitchGenerator.Attenuation).ToString(CultureInfo.InvariantCulture);
        }

        private void MuteVolume()
        {
            pitchGenerator.Attenuation = -maxVolume;
            CurrentVolume = (maxVolume + pitchGenerator.Attenuation).ToString(CultureInfo.InvariantCulture);
        }

        protected override async void OnActivate()
        {
            if (string.IsNullOrEmpty(PresetFileName))
                return;

            maxVolume = (int)settings["MaxVolume"];
            MuteVolume();
            NotifyButtons();

            itemsFileManager.FileName = PresetFileName;
            eventAggregator.Publish(new Events.HearingTest.PitchGeneratorChanged(pitchGenerator));
            eventAggregator.Publish(new Events.HearingTest.ChannelChanged(CurrentChannel));

            presetItems = (await itemsFileManager.GetAsync()).ToList();
            presetQueue = new Queue<PresetItemViewModel>(presetItems);
            if (presetQueue.Count == 0)
                return;

            currentItem = presetQueue.Dequeue();

            CurrentFrequency = currentItem.Frequency.ToString(CultureInfo.InvariantCulture);
        }

        private void NotifyButtons()
        {
            NotifyOfPropertyChange(() => CanIncreaseVolume);
            NotifyOfPropertyChange(() => CanDecreaseVolume);
        }

        public bool CanIncreaseVolume
        {
            get { return (maxVolume + pitchGenerator.Attenuation < maxVolume); }
        }

        public bool CanDecreaseVolume
        {
            get { return (maxVolume + pitchGenerator.Attenuation > 0); }
        }

        public void IncreaseVolume()
        {
            //pitchGenerator.Attenuation = pitchGenerator.Attenuation + 5;
            ChangeVolume(5);
            NotifyButtons();
        }

        public void DecreaseVolume()
        {
            //pitchGenerator.Attenuation = pitchGenerator.Attenuation - 5;
            ChangeVolume(-5);
            NotifyButtons();
        }

    }
}
