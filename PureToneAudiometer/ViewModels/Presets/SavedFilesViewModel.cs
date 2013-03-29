namespace PureToneAudiometer.ViewModels.Presets
{
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using Caliburn.Micro;
    using Windows.Storage;
    using System.Linq;
    using System;

    public class SavedFilesViewModel : ViewModelBase, IHandle<Events.PresetScheduledForDeletion>
    {
        private IObservableCollection<FileViewModel> savedFileList;
        public IObservableCollection<FileViewModel> SavedFileList
        {
            get { return savedFileList; }
            private set
            {
                if (Equals(value, savedFileList)) return;
                savedFileList = value;
                NotifyOfPropertyChange(() => SavedFileList);
            }
        }

        private readonly IEventAggregator eventAggregator;

        public SavedFilesViewModel(IEventAggregator eventAggregator, INavigationService navigationService) : base(navigationService)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            SavedFileList = new BindableCollection<FileViewModel>();
        }

        protected override async void OnActivate()
        {
            await FetchFiles();
        }

        private async Task FetchFiles()
        {
            SavedFileList.Clear();
            var localFolder = ApplicationData.Current.LocalFolder;

            var allFiles = await localFolder.GetFilesAsync();
            var presetFiles = allFiles.Where(x => x.Name.EndsWith(".preset"));

            SavedFileList.AddRange(presetFiles.Select(x => new FileViewModel(eventAggregator)
            {
                PresetName = Path.GetFileNameWithoutExtension(x.Name),
                CreationDate = x.DateCreated.DateTime
            }));
        }

        public async void Handle(Events.PresetScheduledForDeletion message)
        {
            //show the 'Are you sure?' message box etc
            var result = MessageBox.Show("Are you sure you want to delete this preset? [" + message.FileName + "]",
                                         "Deleting preset", MessageBoxButton.OKCancel);

            if (result != MessageBoxResult.OK)
                return;

            var localFolder = ApplicationData.Current.LocalFolder;

            try
            {
                var file = await localFolder.GetFileAsync(message.FileName + ".preset");
                await file.DeleteAsync();
            }
            catch (FileNotFoundException)
            {
            }

            await FetchFiles();
        }

        public void SelectionChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            var selectedItem = e.AddedItems[0] as FileViewModel;
            
            if (selectedItem == null)
                return;
            
            eventAggregator.Publish(new Events.SelectNewPreset(selectedItem.PresetName));
        }
    }
}
