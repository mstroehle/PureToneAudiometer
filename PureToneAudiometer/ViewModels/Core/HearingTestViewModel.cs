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
        private const string PlayGlyph = "";
        private const string PauseGlyph = "";

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
                eventAggregator.Publish(new Events.HearingTest.ChannelChanged(value));
                switch (value)
                {
                    case Channel.Left: 
                        pitchGenerator.MutedChannel = Channel.Right;
                        break;
                    case Channel.Right:
                        pitchGenerator.MutedChannel = Channel.Left;
                        break;
                    case Channel.Both:
                        pitchGenerator.MutedChannel = Channel.None;
                        break;
                    default:
                        pitchGenerator.MutedChannel = Channel.Both;
                        break;
                }
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

        public bool IsPlaying
        {
            get { return isPlaying; }
            set 
            { 
                isPlaying = value;
                PlayContent = value ? PauseGlyph : PlayGlyph;
                NotifyOfPropertyChange(() => PlayContent);
            }
        }

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

        private readonly ISettings settings;
        private int maxVolume;
        private string presetFileName;
        private bool isLeftChannelChecked;
        private bool isRightChannelChecked;
        private IReadOnlyTraversableList<PresetItemViewModel> presetItems;
        private PresetItemViewModel currentItem;
        private int progressMaximum;
        private int progressValue;
        private bool isPlaying;

        public HearingTestViewModel(INavigationService navigationService, 
            IAsyncXmlFileManager fileManager, 
            ISettings settings,
            IEventAggregator eventAggregator, 
            IPitchGenerator pitchGenerator) : base(navigationService)
        {
            this.settings = settings;
            this.pitchGenerator = pitchGenerator;
            this.eventAggregator = eventAggregator;
            this.fileManager = fileManager;

            IsPlaying = false;
            IsLeftChannelChecked = true;
            results = new Dictionary<Channel, ISet<HearingResult>>(2);
            results[Channel.Right] = new HashSet<HearingResult>(new FrequencyResultComparer());
            results[Channel.Left] = new HashSet<HearingResult>(new FrequencyResultComparer());            
            PresetItems = new ReadOnlyTraversableList<PresetItemViewModel>();
        }

        public void Play()
        {
            IsPlaying = !IsPlaying;

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
            IsPlaying = false;
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
            var testResult = new HearingTestResult(results[Channel.Left], results[Channel.Right]) {MaxVolume = maxVolume};
            fileManager.FileName = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".result";
            await fileManager.Save(testResult);
            NavigationService.UriFor<ResultsPageViewModel>().WithParam(x => x.ResultFileName, fileManager.FileName).Navigate();
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
            NotifyOfPropertyChange(() => CanDecreaseVolume);
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
            IsPlaying = false;

            CurrentItem = PresetItems.Next();

            pitchGenerator.Frequency = CurrentItem.Frequency;
            CurrentFrequency = CurrentItem.Frequency.ToString(CultureInfo.InvariantCulture);
            MuteVolume();
            eventAggregator.Publish(new Events.HearingTest.StartPlaying(CurrentItem));
            IsPlaying = true;
        }

        public bool CanPreviousFrequency
        {
            get { return PresetItems.CanGetPrevious; }
        }

        public void PreviousFrequency()
        {
            eventAggregator.Publish(new Events.HearingTest.StopPlaying());
            IsPlaying = false;

            CurrentItem = PresetItems.Previous();

            pitchGenerator.Frequency = CurrentItem.Frequency;
            CurrentFrequency = CurrentItem.Frequency.ToString(CultureInfo.InvariantCulture);
            MuteVolume();
            eventAggregator.Publish(new Events.HearingTest.StartPlaying(CurrentItem));
            IsPlaying = true;
        }

        protected override async void OnActivate()
        {
            if (string.IsNullOrEmpty(PresetFileName))
                return;
            maxVolume = settings.Get<int>("MaxVolume").GetOrElse(100);
            MuteVolume();
            NotifyButtons();

            fileManager.FileName = PresetFileName;
            eventAggregator.Publish(new Events.HearingTest.PitchGeneratorChanged(pitchGenerator));
            eventAggregator.Publish(new Events.HearingTest.ChannelChanged(CurrentChannel));
            eventAggregator.Publish(new Events.HearingTest.StopPlaying());
            IsPlaying = false;

            PresetItems = new ReadOnlyTraversableList<PresetItemViewModel>((await fileManager.GetCollection<PresetItemViewModel>()).ToList());
            ProgressMaximum = PresetItems.Count;
            if (!PresetItems.Any())
                return;

            CurrentItem = PresetItems.Next();
            pitchGenerator.Frequency = CurrentItem.Frequency;
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
