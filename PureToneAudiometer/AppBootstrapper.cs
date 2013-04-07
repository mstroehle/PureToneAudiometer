namespace PureToneAudiometer
{
    using System.Collections.Generic;
    using System.IO.IsolatedStorage;
    using System.Windows.Controls;
    using Audio;
    using Caliburn.Micro;
    using Caliburn.Micro.BindableAppBar;
    using ViewModels;
    using ViewModels.Core;
    using ViewModels.Presets;
    using ViewModels.Start;
    using Views.Core;
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
            container.PerRequest<RecentPageViewModel>();
            container.PerRequest<MainPageViewModel>();
            container.Handler<IDictionary<string, object>>(
                simpleContainer => IsolatedStorageSettings.ApplicationSettings);
            container.PerRequest<SettingsPageViewModel>();
            container.PerRequest<HearingTestViewModel>();
            container.PerRequest<ChannelSelectionPageViewModel>();
            container.PerRequest<HostPageViewModel>();
            container.PerRequest<HearingTestView>();
            container.Handler<IStorageFolder>(simpleContainer => ApplicationData.Current.LocalFolder);
            container.Handler<IXmlItemsFileManager<RecentItemViewModel>>(
                simpleContainer =>
                new RecentItemManager((IStorageFolder) simpleContainer.GetInstance(typeof (IStorageFolder), null),
                                      "recent.xml"));
            container.Handler<IXmlItemsFileManager<PresetItemViewModel>>(
                simpleContainer =>
                new XmlItemsFileManager<PresetItemViewModel>(
                    (IStorageFolder) simpleContainer.GetInstance(typeof (IStorageFolder), null), "default.preset"));
            container.RegisterPerRequest(typeof(AddItemViewModel), "AddItemViewModel", typeof(AddItemViewModel));
            container.Handler<IOscillator>(simpleContainer => new SineOscillator(-95, 100));
            container.Handler<IPitchGenerator>(
                simpleContainer =>
                new PitchGenerator((IOscillator) simpleContainer.GetInstance(typeof (IOscillator), null)));
            container.RegisterPhoneServices();
            
            AddConventions();
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
