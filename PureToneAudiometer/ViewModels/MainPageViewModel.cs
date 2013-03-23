namespace PureToneAudiometer.ViewModels
{
    using Caliburn.Micro;

    public class MainPageViewModel :  ViewModelBase
    {
        public MainPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            navigationService.UriFor<PresetsPageViewModel>().Navigate();
        }
    }
}
