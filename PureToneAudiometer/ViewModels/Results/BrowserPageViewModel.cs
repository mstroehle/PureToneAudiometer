namespace PureToneAudiometer.ViewModels.Results
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using Caliburn.Micro;
    using Core;
    using Windows.Storage;
    using System;

    public class BrowserPageViewModel : ViewModelBase, IHandle<Events.ResultItemTapped>
    {
        public IObservableCollection<ResultFileViewModel> ResultFiles { get; private set; }

        public bool SelectionEnabled
        {
            get { return selectionEnabled; }
            set
            {
                if (value.Equals(selectionEnabled)) return;
                selectionEnabled = value;
                NotifyOfPropertyChange(() => SelectionEnabled);
            }
        }

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (value.Equals(isBusy)) return;
                isBusy = value;
                NotifyOfPropertyChange(() => IsBusy);
            }
        }

        private readonly IStorageFolder storageFolder;
        private bool isBusy;

        private readonly IAsyncXmlFileManager fileManager;
        private bool selectionEnabled;
        private readonly ISet<ResultFileViewModel> selectedItems;
        private readonly IEventAggregator eventAggregator;
        public BrowserPageViewModel(IStorageFolder appStorageFolder, INavigationService navigationService, IAsyncXmlFileManager xmlFileManager, IEventAggregator eventAggregator) : base(navigationService)
        {
            this.eventAggregator = eventAggregator;
            this.eventAggregator.Subscribe(this);
            ResultFiles = new BindableCollection<ResultFileViewModel>();
            selectedItems = new HashSet<ResultFileViewModel>();
            storageFolder = appStorageFolder;
            fileManager = xmlFileManager;
        }

        protected async override void OnActivate()
        {
            IsBusy = true;
            ResultFiles.Clear();   
            var files = await storageFolder.GetFilesAsync();

            var resultFiles = files.Where(x => x.Name.EndsWith(".result", StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var storageFile in resultFiles)
            {
                var modificationDate = (await storageFile.GetBasicPropertiesAsync()).DateModified;
                fileManager.FileName = storageFile.Name;
                var testResult = await fileManager.Get<TestResult>();
                ResultFiles.Add(new ResultFileViewModel(eventAggregator)
                                       {
                                           CreationDate = storageFile.DateCreated.DateTime,
                                           LastChangedDate = modificationDate.DateTime,
                                           FileName = storageFile.Name,
                                           Description = testResult.Description
                                       });
            }
            IsBusy = false;
        }

        public void EnableSelection()
        {
            SelectionEnabled = true;
        }

        public async void DeleteSelected()
        {
            var result = MessageBox.Show("Are you sure you want to remove selected items?", "Removal confirmation",
                                      MessageBoxButton.OKCancel);
            
            if (result != MessageBoxResult.Yes)
                return;

            foreach (var item in selectedItems)
            {
                ResultFiles.Remove(item);
                var file = await storageFolder.GetFileAsync(item.FileName);
                await file.DeleteAsync();
            }
            SelectionEnabled = false;
        }

        public void SelectionChanged(SelectionChangedEventArgs e)
        {
            var addedItems = e.AddedItems.Cast<ResultFileViewModel>().ToList();
            var removedItems = e.RemovedItems.Cast<ResultFileViewModel>().ToList();

            foreach (var resultFileViewModel in addedItems)
            {
                selectedItems.Add(resultFileViewModel);
            }

            foreach (var resultFileViewModel in removedItems)
            {
                selectedItems.Remove(resultFileViewModel);
            }
        }

        public void BackKeyPress(CancelEventArgs eventArgs)
        {
            if (SelectionEnabled)
            {
                SelectionEnabled = false;
                eventArgs.Cancel = true;
            }
        }

        public void Handle(Events.ResultItemTapped message)
        {
            NavigationService.UriFor<TestResultsPageViewModel>().WithParam(x => x.ResultFileName, message.FileName).Navigate();
        }
    }
}
