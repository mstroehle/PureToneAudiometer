namespace PureToneAudiometer
{
    using System.Windows.Controls;
    using Caliburn.Micro;
    using Caliburn.Micro.BindableAppBar;
    using ViewModels;
    using ViewModels.Presets;

    public class AppBootstrapper : PhoneBootstrapper
    {
        private PhoneContainer container;

        protected override void Configure()
        {
            container = new PhoneContainer(RootFrame);
            container.Singleton<IEventAggregator, EventAggregator>();
            container.PerRequest<MainPageViewModel>();
            container.PerRequest<PresetsPageViewModel>();
            container.PerRequest<PresetViewModel>();
            container.PerRequest<SavedFilesViewModel>();
            container.RegisterPerRequest(typeof(AddItemViewModel), "AddItemViewModel", typeof(AddItemViewModel));
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

        protected override System.Collections.Generic.IEnumerable<object> GetAllInstances(System.Type service)
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
