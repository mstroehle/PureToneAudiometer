namespace PureToneAudiometer.ViewModels
{
	using System.Collections.ObjectModel;
	using System.Windows.Controls;
	using Caliburn.Micro;

	public class StartPageViewModel : ViewModelBase
	{
		public ReadOnlyObservableCollection<NavigationItem> NavigationItems { get; private set; }

		public StartPageViewModel(INavigationService navigationService)
			: base(navigationService)
		{
			NavigationItems =
				new ReadOnlyObservableCollection<NavigationItem>(
					new ObservableCollection<NavigationItem>(new[]
						                                         {
							                                         new NavigationItem
								                                         {
									                                         Glyph = "&#x2649",
									                                         NavigationName = "Presets",
									                                         NavigationAction =
										                                         () =>
										                                         NavigationService.UriFor<PresetsPageViewModel>()
										                                                          .Navigate()
								                                         },
							                                         new NavigationItem
								                                         {
									                                         Glyph = "&#x269C",
									                                         NavigationName = "Settings",
									                                         NavigationAction = () => { }
								                                         }
						                                         }));
		}

		public void SelectionChanged(SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count == 0)
				return;

			var addedItem = e.AddedItems[0] as NavigationItem;

			if (addedItem == null)
				return;

			addedItem.NavigationAction();
		}

	}
}
