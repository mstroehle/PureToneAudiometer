namespace PureToneAudiometer.ViewModels.Presets
{
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using Caliburn.Micro;
    using Core;
    using Start;
    using Windows.Storage;
    using System.Linq;
    using System;

    public class SavedFilesViewModel : ViewModelBase, IHandle<Events.PresetScheduledForDeletion>, IHandle<Events.UsePreset>
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

        private readonly IAsyncXmlFileManager recentManager;

        private readonly IStorageFolder storageFolder;

        public SavedFilesViewModel(IEventAggregator eventAggregator, IStorageFolder storageFolder, INavigationService navigationService, IAsyncXmlFileManager recentManager)
            : base(navigationService)
        {
            this.storageFolder = storageFolder;
            this.recentManager = recentManager;
            this.eventAggregator = eventAggregator;
            SavedFileList = new BindableCollection<FileViewModel>();
        }

        protected override async void OnActivate()
        {
            eventAggregator.Subscribe(this);
            await FetchFiles();
        }

        protected override void OnDeactivate(bool close)
        {
            eventAggregator.Unsubscribe(this);
        }

        private async Task FetchFiles()
        {
            SavedFileList.Clear();

            var allFiles = await storageFolder.GetFilesAsync();
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

            try
            {
                var file = await storageFolder.GetFileAsync(message.FileName);
                recentManager.FileName = "recent.xml";
                await file.DeleteAsync();
                await recentManager.RemoveFromCollection<RecentItemViewModel>(x => x.FilePath == message.FileName);
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

        public void Handle(Events.UsePreset message)
        {
            var recent = new RecentItemViewModel
                             {
                                 FilePath = message.FileName,
                                 PresetName = Path.GetFileNameWithoutExtension(message.FileName),
                                 LastUsedDate = DateTime.Now
                             };
            recentManager.FileName = "recent.xml";
            recentManager.UpdateOrAddToCollection(recent, model => model.FilePath == message.FileName);
            NavigationService.UriFor<HostPageViewModel>().WithParam(x => x.PresetFileName, message.FileName).Navigate();
            
            // NavigationService.UriFor<ChannelSelectionPageViewModel>().WithParam(x => x.PresetFileName, message.FileName).Navigate();
        }
    }
}
