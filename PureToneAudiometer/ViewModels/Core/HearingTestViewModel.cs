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
    using Results;

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

        public bool IsLeftChannelChecked
        {
            get { return isLeftChannelChecked; }
            set
            {
                if (value.Equals(isLeftChannelChecked)) return;
                isLeftChannelChecked = value;
                NotifyOfPropertyChange(() => IsLeftChannelChecked);
                if(value)
                    CurrentChannel = Channel.Left;
            }
        }

        public bool IsRightChannelChecked
        {
            get { return isRightChannelChecked; }
            set
            {
                if (value.Equals(isRightChannelChecked)) return;
                isRightChannelChecked = value;
                NotifyOfPropertyChange(() => IsRightChannelChecked);
                if(value)
                    CurrentChannel = Channel.Right;
            }
        }

        public int ProgressMaximum
        {
            get { return progressMaximum; }
            set
            {
                if (value == progressMaximum) return;
                progressMaximum = value;
                NotifyOfPropertyChange(() => ProgressMaximum);
            }
        }

        public int ProgressValue
        {
            get { return progressValue; }
            set
            {
                if (value == progressValue) return;
                progressValue = value;
                NotifyOfPropertyChange(() => ProgressValue);
            }
        }

        private readonly IAsyncXmlFileManager fileManager;
        private IReadOnlyTraversableList<PresetItemViewModel> PresetItems
        {
            get { return presetItems; }
            set
            {
                presetItems = value;
                NotifyOfPropertyChange(() => CanPreviousFrequency);
                NotifyOfPropertyChange(() => CanNextFrequency);
            }
        }

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

        private readonly IDictionary<Channel, ISet<HearingResult>> results;
        private readonly IPitchGenerator pitchGenerator;
        private PresetItemViewModel CurrentItem
        {
            get { return currentItem; }
            set
            {
                currentItem = value;
                NotifyOfPropertyChange(() => CanPreviousFrequency);
                NotifyOfPropertyChange(() => CanNextFrequency);
            }
        }

        private readonly IDictionary<string, object> settings;
        private int maxVolume;
        private string presetFileName;
        private bool isLeftChannelChecked;
        private bool isRightChannelChecked;
        private IReadOnlyTraversableList<PresetItemViewModel> presetItems;
        private PresetItemViewModel currentItem;
        private int progressMaximum;
        private int progressValue;

        public HearingTestViewModel(INavigationService navigationService, 
            IAsyncXmlFileManager fileManager, 
            IDictionary<string, object> settings,
            IEventAggregator eventAggregator, 
            IPitchGenerator pitchGenerator) : base(navigationService)
        {
            this.settings = settings;
            this.pitchGenerator = pitchGenerator;
            IsPlaying = false;
            PlayContent = IsPlaying ? "Pause" : "Play";
            IsLeftChannelChecked = true;
            results = new Dictionary<Channel, ISet<HearingResult>>(2);
            results[Channel.Right] = new HashSet<HearingResult>(new FrequencyResultComparer());
            results[Channel.Left] = new HashSet<HearingResult>(new FrequencyResultComparer());
            this.eventAggregator = eventAggregator;
            PresetItems = new ReadOnlyTraversableList<PresetItemViewModel>();
            this.fileManager = fileManager;
        }

        public void Play()
        {
            IsPlaying = !IsPlaying;

            PlayContent = IsPlaying ? "Pause" : "Play";

            if(IsPlaying && CurrentItem != null)
                eventAggregator.Publish(new Events.HearingTest.StartPlaying(CurrentItem));
            else
            {
                eventAggregator.Publish(new Events.HearingTest.StopPlaying());
            }
        }

        public void Ok()
        {
            eventAggregator.Publish(new Events.HearingTest.StopPlaying());
            var result = new HearingResult
                             {
                                 Frequency = CurrentItem.Frequency,
                                 Volume = int.Parse(CurrentVolume)
                             };
            results[CurrentChannel].Remove(result);
            results[CurrentChannel].Add(result);

            var sum = results[Channel.Left].Count + results[Channel.Right].Count;
            var denominator = 2.0*PresetItems.Count;

            var fraction = sum/denominator;

            ProgressValue = (int)(fraction * 100);
        }
        
        public async void GoToResults()
        {
            Deactivate();
            var testresult = new TestResult(results[Channel.Left], results[Channel.Right]);
            fileManager.FileName = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".result";
            await fileManager.Save(testresult);
            NavigationService.UriFor<TestResultsPageViewModel>().WithParam(x => x.ResultFileName, fileManager.FileName).Navigate();
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

        public void Deactivate()
        {
            eventAggregator.Publish(new Events.HearingTest.Deactivate());
        }

        public bool CanNextFrequency
        {
            get { return PresetItems.CanGetNext; }
        }

        public void NextFrequency()
        {
            eventAggregator.Publish(new Events.HearingTest.StopPlaying());

            CurrentItem = PresetItems.Next();

            pitchGenerator.Frequency = CurrentItem.Frequency;
            CurrentFrequency = CurrentItem.Frequency.ToString(CultureInfo.InvariantCulture);
            MuteVolume();
            eventAggregator.Publish(new Events.HearingTest.StartPlaying(CurrentItem));
        }

        public bool CanPreviousFrequency
        {
            get { return PresetItems.CanGetPrevious; }
        }

        public void PreviousFrequency()
        {
            eventAggregator.Publish(new Events.HearingTest.StopPlaying());

            CurrentItem = PresetItems.Previous();

            pitchGenerator.Frequency = CurrentItem.Frequency;
            CurrentFrequency = CurrentItem.Frequency.ToString(CultureInfo.InvariantCulture);
            MuteVolume();
            eventAggregator.Publish(new Events.HearingTest.StartPlaying(CurrentItem));
        }

        protected override async void OnActivate()
        {
            if (string.IsNullOrEmpty(PresetFileName))
                return;
            maxVolume = (int)settings["MaxVolume"];
            MuteVolume();
            NotifyButtons();

            fileManager.FileName = PresetFileName;
            eventAggregator.Publish(new Events.HearingTest.PitchGeneratorChanged(pitchGenerator));
            eventAggregator.Publish(new Events.HearingTest.ChannelChanged(CurrentChannel));

            PresetItems = new ReadOnlyTraversableList<PresetItemViewModel>((await fileManager.GetCollection<PresetItemViewModel>()).ToList());
            ProgressMaximum = PresetItems.Count;
            if (!PresetItems.Any())
                return;

            CurrentItem = PresetItems.Next();

            CurrentFrequency = CurrentItem.Frequency.ToString(CultureInfo.InvariantCulture);
            NotifyButtons();
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
            ChangeVolume(1);
            NotifyButtons();
        }

        public void DecreaseVolume()
        {
            ChangeVolume(-1);
            NotifyButtons();
        }

    }
}
