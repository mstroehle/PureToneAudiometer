namespace PureToneAudiometer.ViewModels
{
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using Caliburn.Micro;
    using Coding4Fun.Toolkit.Controls;
    using Start;

    public class MainPageViewModel : Screen
    {
        public RecentPageViewModel Recent { get; private set; }
        public MainMenuPageViewModel MainMenu { get; private set; }
        private readonly INavigationService navigationService;
        private readonly ISettings settings;

        public MainPageViewModel(INavigationService navigationService, RecentPageViewModel recent, MainMenuPageViewModel mainMenu, ISettings settings)
        {
            this.navigationService = navigationService;    
            Recent = recent;
            MainMenu = mainMenu;
            this.settings = settings;
        }

        protected override async void OnActivate()
        {
            await Recent.Initialize();

            var firstRun = settings.Get<bool>("FirstRun").GetOrElse(true);
            if (!firstRun)
                return;
            var toast = new ToastPrompt
            {
                Title = "First run",
                TextOrientation = Orientation.Horizontal,
                Message = "Please check the 'Help' section. Tap here if you want to go there now",
                TextWrapping = TextWrapping.Wrap
            };
            toast.Completed += (sender, args) =>
                                   {
                                       if (args.PopUpResult == PopUpResult.Ok)
                                           navigationService.UriFor<HelpPageViewModel>().Navigate();
                                   };
            toast.Show();
            settings.Set("FirstRun", false);
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
