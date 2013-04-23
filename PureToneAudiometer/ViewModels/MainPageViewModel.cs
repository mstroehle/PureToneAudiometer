namespace PureToneAudiometer.ViewModels
{
    using System.ComponentModel;
    using System.Linq;
    using Caliburn.Micro;
    using Start;

    public class MainPageViewModel : Screen
    {
        public RecentPageViewModel Recent { get; private set; }
        public MainMenuPageViewModel MainMenu { get; private set; }
        private readonly INavigationService navigationService;

        public MainPageViewModel(INavigationService navigationService, RecentPageViewModel recent, MainMenuPageViewModel mainMenu)
        {
            this.navigationService = navigationService;    
            Recent = recent;
            MainMenu = mainMenu;
        }

        protected override async void OnActivate()
        {
            await Recent.Initialize();
        }

        public void BackKeyPressed(CancelEventArgs e)
        {
            while (navigationService.BackStack.Any())
            {
                navigationService.RemoveBackEntry();
            }
        }
    }
}
