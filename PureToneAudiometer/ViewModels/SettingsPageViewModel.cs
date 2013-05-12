namespace PureToneAudiometer.ViewModels
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using Caliburn.Micro;

    public class SettingsPageViewModel : ViewModelBase
    {
        private string maxVolume;
        public string MaxVolume
        {
            get { return maxVolume; }
            set
            {
                if (value == maxVolume) return;
                maxVolume = value;

                int val;
                if (int.TryParse(maxVolume, out val))
                {
                    settings.Set("MaxVolume", val);
                }

                NotifyOfPropertyChange(() => MaxVolume);
            }
        }

        public bool ShouldUploadPlots
        {
            get { return shouldUploadPlots; }
            set
            {
                if (value.Equals(shouldUploadPlots)) return;
                shouldUploadPlots = value;
                settings.Set("ShouldAutomaticallyUploadPlots", value);
                NotifyOfPropertyChange(() => ShouldUploadPlots);
            }
        }

        public string RecentItemsShown
        {
            get { return recentItemsShown; }
            set
            {
                if (Equals(value, recentItemsShown)) return;
                recentItemsShown = value;
                settings.Set("RecentItemsShown", value);
                NotifyOfPropertyChange(() => RecentItemsShown);
            }
        }

        public IReadOnlyList<string> RecentItems { get; private set; }

        private readonly ISettings settings;
        private bool shouldUploadPlots;
        private string recentItemsShown;

        public SettingsPageViewModel(ISettings settings, INavigationService navigationService) : base(navigationService)
        {
            this.settings = settings;
            RecentItems =
                new List<string>(new[]
                                     {
                                         "None",
                                         "5", "10",
                                         "15", "20",
                                         "No limit"
                                     });
            MaxVolume = settings.Get<int>("MaxVolume").Select(x => x.ToString(CultureInfo.InvariantCulture)).GetOrElse("100");
            ShouldUploadPlots = settings.Get<bool>("ShouldAutomaticallyUploadPlots").GetOrElse(false);
            RecentItemsShown =
                settings.Get<string>("RecentItemsShown")
                        .GetOrElse(() => RecentItems.Last());
        }

        public void ClearSettings()
        {
            var result = MessageBox.Show("Are you sure you want to clear all settings and revert them to default?",
                                     "Confirmation", MessageBoxButton.OKCancel);
            if (result != MessageBoxResult.OK)
                return;

            settings.Clear();

            var dict = new Dictionary<string, object>
                           {
                               {"MaxVolume", 100},
                               {"ShouldAutomaticallyUploadPlots", false},
                               {"RecentItemsShown", "No limit"}
                           };

            settings.MergeOverwrite(dict);

            NavigationService.UriFor<MainPageViewModel>().Navigate();
        }
    }
}
