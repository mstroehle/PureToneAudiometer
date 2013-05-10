namespace PureToneAudiometer
{
    using System.Collections.Generic;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Windows.Controls;
    using Audio;
    using Caliburn.Micro;
    using Caliburn.Micro.BindableAppBar;
    using ViewModels;
    using ViewModels.Core;
    using ViewModels.Live;
    using ViewModels.Presets;
    using ViewModels.Results;
    using ViewModels.Start;
    using Views.Core;
    using Views.Presets;
    using Views.Results;
    using Windows.Storage;

    public class AppBootstrapper : PhoneBootstrapper
    {
        private PhoneContainer container;

        protected override void Configure()
        {
            container = new PhoneContainer(RootFrame);
            container.Singleton<IEventAggregator, EventAggregator>();
            container.PerRequest<AudiometerPageViewModel>();
            container.PerRequest<PresetsPageViewModel>();
            container.PerRequest<PresetViewModel>();
            container.PerRequest<SavedFilesViewModel>();
            container.PerRequest<MainMenuPageViewModel>();
          
            container.PerRequest<MainPageViewModel>();
            container.Handler<IDictionary<string, object>>(
                simpleContainer => IsolatedStorageSettings.ApplicationSettings);
            container.PerRequest<SettingsPageViewModel>();
            container.PerRequest<HearingTestViewModel>();
            container.PerRequest<HostPageViewModel>();
            container.PerRequest<HearingTestView>();
            container.PerRequest<RecentPageViewModel>();
            container.PerRequest<MainLivePageViewModel>();
            container.PerRequest<BrowserPageViewModel>();
            container.Handler<IStorageFolder>(simpleContainer => ApplicationData.Current.LocalFolder);
            container.PerRequest<IAsyncXmlFileManager, AsyncXmlFileManager>();
            container.PerRequest<ISkyDriveUpload, SkyDriveUpload>();
            container.PerRequest<TestResultsPageViewModel>();
            container.RegisterPerRequest(typeof(AddItemViewModel), "AddItemViewModel", typeof(AddItemViewModel));
            container.RegisterPerRequest(typeof(SaveResultViewModel), "SaveResultViewModel", typeof(SaveResultViewModel));
            container.PerRequest<IDialogBuilder<AddItemView, AddItemViewModel>, DialogBuilder<AddItemView, AddItemViewModel>>();
            container.PerRequest<IDialogBuilder<SaveResultView, SaveResultViewModel>, DialogBuilder<SaveResultView, SaveResultViewModel>>();
            container.Handler<IOscillator>(simpleContainer => new SineOscillator(-95, 100));
            container.Handler<IPitchGenerator>(
                simpleContainer =>
                new PitchGenerator((IOscillator) simpleContainer.GetInstance(typeof (IOscillator), null)));
            container.RegisterPhoneServices();
            AddDefaultSettings();
            AddConventions();
        }

        private void AddDefaultSettings()
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Any())
            {
                IsolatedStorageSettings.ApplicationSettings["MaxVolume"] = 100;
                IsolatedStorageSettings.ApplicationSettings["MaxRecentItems"] = 5;
            }
        }

        private void AddConventions()
        {
            ConventionManager.AddElementConvention<BindableAppBarButton>(Control.IsEnabledProperty, "DataContext", "Click");
            ConventionManager.AddElementConvention<BindableAppBarMenuItem>(Control.IsEnabledProperty, "DataContext", "Click");
        }

        protected override object GetInstance(System.Type service, string key)
        {
            return container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(System.Type service)
        {
            return container.GetAllInstances(service);
        }

        protected override void OnUnhandledException(object sender, System.Windows.ApplicationUnhandledExceptionEventArgs e)
        {
            
        }

        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }
    }
}
