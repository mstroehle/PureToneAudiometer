namespace PureToneAudiometer.ViewModels
{
    using Caliburn.Micro;

    public abstract class ViewModelBase : Screen
    {
        private readonly INavigationService navigationService;
        public INavigationService NavigationService
        {
            get { return navigationService; }
        }

        protected ViewModelBase(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }
    }
}
