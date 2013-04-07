namespace PureToneAudiometer.ViewModels.Start
{
    using Caliburn.Micro;

    public class AudiometerPageViewModel :  ViewModelBase
    {
        public AudiometerPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            navigationService.UriFor<PresetsPageViewModel>().Navigate();
        }
    }
}
