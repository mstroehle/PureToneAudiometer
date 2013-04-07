namespace PureToneAudiometer.ViewModels
{
    using Caliburn.Micro;
    using Start;

    public class MainPageViewModel : Screen
    {
        public RecentPageViewModel Recent { get; private set; }
        public MainMenuPageViewModel MainMenu { get; private set; }

        public MainPageViewModel(RecentPageViewModel recent, MainMenuPageViewModel mainMenu)
        {
            Recent = recent;
            MainMenu = mainMenu;
        }

        protected override async void OnActivate()
        {
            await Recent.Initialize();
        }
    }
}
