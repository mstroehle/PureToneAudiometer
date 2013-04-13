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

        private readonly IAsyncXmlFileManager fileManager;
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

        private readonly IDictionary<Channel, IList<HearingResult>> results;
        private readonly IPitchGenerator pitchGenerator;
        private bool firstRun;
        private PresetItemViewModel currentItem;
        private readonly IDictionary<string, object> settings;
        private int maxVolume;
        private string presetFileName;

        public HearingTestViewModel(INavigationService navigationService, 
            IAsyncXmlFileManager fileManager, 
            IDictionary<string, object> settings,
            IEventAggregator eventAggregator, 
            IPitchGenerator pitchGenerator) : base(navigationService)
        {
            this.settings = settings;
            this.pitchGenerator = pitchGenerator;
            IsPlaying = true;
            PlayContent = IsPlaying ? "Pause" : "Play";
            CurrentChannel = Channel.None;
            results = new Dictionary<Channel, IList<HearingResult>>(2);
            results[Channel.Right] = new List<HearingResult>(30);
            results[Channel.Left] = new List<HearingResult>(30);
            this.eventAggregator = eventAggregator;
            presetItems = new List<PresetItemViewModel>(10);
            presetQueue = new Queue<PresetItemViewModel>(10);
            this.fileManager = fileManager;
            firstRun = true;
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

        public async void Ok()
        {
            eventAggregator.Publish(new Events.HearingTest.StopPlaying());
            results[CurrentChannel].Add(new HearingResult
                                            {
                                                Frequency = currentItem.Frequency,
                                                Volume = int.Parse(CurrentVolume)
                                            });
            if (presetQueue.Count > 0)
            {
                currentItem = presetQueue.Dequeue();
                eventAggregator.Publish(new Events.HearingTest.StartPlaying(currentItem));
            }
            else if(firstRun)
            {
                firstRun = false;
                CurrentChannel = CurrentChannel == Channel.Right ? Channel.Left : Channel.Right;
                eventAggregator.Publish(new Events.HearingTest.ChannelChanged(CurrentChannel));
                presetQueue = new Queue<PresetItemViewModel>(presetItems);
                currentItem = presetQueue.Dequeue();
            }
            else
            {
                Deactivate();
                var testResult = new TestResult(results[Channel.Left], results[Channel.Right]) {MaxVolume = maxVolume};
                fileManager.FileName = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".result";
                await fileManager.Save(testResult);
                NavigationService.UriFor<TestResultsPageViewModel>().WithParam(x => x.ResultFileName, fileManager.FileName).Navigate();
                return;
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

        public void Deactivate()
        {
            eventAggregator.Publish(new Events.HearingTest.Deactivate());
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

            presetItems = (await fileManager.GetCollection<PresetItemViewModel>()).ToList();
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
            ChangeVolume(5);
            NotifyButtons();
        }

        public void DecreaseVolume()
        {
            ChangeVolume(-5);
            NotifyButtons();
        }

    }
}
