namespace PureToneAudiometer.ViewModels
{
    using System.Collections.Generic;
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
                    settings["MaxVolume"] = val;
                }

                NotifyOfPropertyChange(() => MaxVolume);
            }
        }

        private readonly IDictionary<string, object> settings;

        public SettingsPageViewModel(IDictionary<string, object> settings, INavigationService navigationService) : base(navigationService)
        {
            this.settings = settings;
         
            object val;
            if (settings.TryGetValue("MaxVolume", out val))
            {
                MaxVolume = val.ToString();
            }
        }
    }
}
