namespace PureToneAudiometer.ViewModels.Core
{
    using Audio;
    using Caliburn.Micro;

    public class HostPageViewModel : ViewModelBase
    {
        public string PresetFileName { get; set; }
        public Channel CurrentChannel { get; set; }

        public HearingTestViewModel HearingTestViewModel { get; private set; }

        public HostPageViewModel(INavigationService navigationService, HearingTestViewModel hearingViewModel) : base(navigationService)
        {
            HearingTestViewModel = hearingViewModel;
        }

        protected override void OnActivate()
        {
            HearingTestViewModel.CurrentChannel = CurrentChannel;
            HearingTestViewModel.PresetFileName = PresetFileName;
            ((IActivate)HearingTestViewModel).Activate();
        }
    }
}
