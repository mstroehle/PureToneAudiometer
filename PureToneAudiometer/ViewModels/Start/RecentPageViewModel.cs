namespace PureToneAudiometer.ViewModels.Start
{
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using Caliburn.Micro;
    using Core;

    public class RecentPageViewModel : ViewModelBase
    {
        public IObservableCollection<RecentItemViewModel> RecentItems { get; private set; }
       
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

        public bool ShouldShowRecent
        {
            get { return shouldShowRecent; }
            private set
            {
                if (value.Equals(shouldShowRecent)) return;
                shouldShowRecent = value;
                NotifyOfPropertyChange(() => ShouldShowRecent);
            }
        }

        private readonly IAsyncXmlFileManager recentManager;
        private readonly ISettings settings;
        private int selectedIndex;
        private bool shouldShowRecent;

        public RecentPageViewModel(INavigationService navigationService, IAsyncXmlFileManager recentManager, ISettings settings) : base(navigationService)
        {
            RecentItems = new BindableCollection<RecentItemViewModel>();
            this.recentManager = recentManager;
            this.settings = settings;
            this.recentManager.FileName = "recent.xml";
        }

        public async Task Initialize()
        {
            RecentItems.Clear();
            ShouldShowRecent = true;
            var result = await recentManager.GetCollection<RecentItemViewModel>();
            var partial = result.OrderByDescending(x => x.LastUsedDate);
            var showRecent = settings.Get<string>("RecentItemsShown").GetOrDefault();
            int takeFirst;
            if (string.IsNullOrEmpty(showRecent) || showRecent.Equals("No limit"))
            {
                RecentItems.AddRange(partial);
            }
            else if(int.TryParse(showRecent, out takeFirst))
            {
                RecentItems.AddRange(partial.Take(takeFirst));
            }
            else
            {
                ShouldShowRecent = false;
            }
        }

        public void SelectionChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            var item = (RecentItemViewModel)e.AddedItems[0];
            NavigationService.UriFor<HostPageViewModel>().WithParam(x => x.PresetFileName, item.FilePath).Navigate();
            SelectedIndex = -1;
        }
    }
}
