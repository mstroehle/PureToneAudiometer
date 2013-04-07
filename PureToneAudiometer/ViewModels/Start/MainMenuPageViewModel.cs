namespace PureToneAudiometer.ViewModels.Start
{
    using System.Collections.ObjectModel;
    using System.Windows.Controls;
    using Caliburn.Micro;

    public class MainMenuPageViewModel : ViewModelBase
	{
	    private int selectedIndex;
	    public ReadOnlyObservableCollection<NavigationItem> NavigationItems { get; private set; }

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                if (value == selectedIndex) return;
                selectedIndex = value;
                NotifyOfPropertyChange(() => SelectedIndex);
            }
        }

	    public MainMenuPageViewModel(INavigationService navigationService)
			: base(navigationService)
	    {
	        NavigationItems =
	            new ReadOnlyObservableCollection<NavigationItem>(
	                new ObservableCollection<NavigationItem>(new[]
	                                                             {
	                                                                 new NavigationItem
	                                                                     {
	                                                                         Glyph = "",
	                                                                         NavigationName = "Presets",
	                                                                         NavigationAction =
	                                                                             () =>
	                                                                             NavigationService
	                                                                                 .UriFor<PresetsPageViewModel>()
	                                                                                 .Navigate(),
	                                                                         Description = "Manage and select presets"
	                                                                     },
	                                                                 new NavigationItem
	                                                                     {
	                                                                         Glyph = "",
	                                                                         NavigationName = "Settings",
	                                                                         NavigationAction =
	                                                                             () =>
	                                                                             NavigationService
	                                                                                 .UriFor<SettingsPageViewModel>()
	                                                                                 .Navigate(),
	                                                                         Description = "Change appplication settings"
	                                                                     }
	                                                             }));
	        SelectedIndex = -1;
		}

		public void SelectionChanged(SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count == 0)
				return;

			var addedItem = e.AddedItems[0] as NavigationItem;

			if (addedItem == null)
				return;

            SelectedIndex = -1;
            
			addedItem.NavigationAction();
		}

	}
}
