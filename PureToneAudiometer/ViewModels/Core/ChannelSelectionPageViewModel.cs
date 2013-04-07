namespace PureToneAudiometer.ViewModels.Core
{
    using Audio;
    using Caliburn.Micro;

    public class ChannelSelectionPageViewModel : ViewModelBase
    {
        public string PresetFileName { get; set; }

        public ChannelSelectionPageViewModel(INavigationService navigationService) : base(navigationService)
        {
        }

        public void RightEar()
        {
            NavigationService.UriFor<HostPageViewModel>()
                             .WithParam(x => x.PresetFileName, PresetFileName)
                             .WithParam(x => x.CurrentChannel, Channel.Right)
                             .Navigate();
        }

        public void LeftEar()
        {
            NavigationService.UriFor<HostPageViewModel>()
                             .WithParam(x => x.PresetFileName, PresetFileName)
                             .WithParam(x => x.CurrentChannel, Channel.Left)
                             .Navigate();
        }
    }
}
