namespace PureToneAudiometer.ViewModels.Results
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Caliburn.Micro;
    using Coding4Fun.Toolkit.Controls;
    using Core;
    using Microsoft.Live;
    using Windows.Storage;
    using System;

    public class BrowserPageViewModel : ViewModelBase, IProgress<LiveOperationProgress>
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

        public bool IsUploading
        {
            get { return isUploading; }
            private set
            {
                if (value.Equals(isUploading)) return;
                isUploading = value;
                NotifyOfPropertyChange(() => IsUploading);
            }
        }

        public bool IsUsingLive
        {
            get { return isUsingLive; }
            private set
            {
                if (value.Equals(isUsingLive)) return;
                isUsingLive = value;
                NotifyOfPropertyChange(() => IsUsingLive);
            }
        }

        public string UploadMessage
        {
            get { return uploadMessage; }
            private set
            {
                if (value == uploadMessage) return;
                uploadMessage = value;
                NotifyOfPropertyChange(() => UploadMessage);
            }
        }

        public double UploadPercentage
        {
            get { return uploadPercentage; }
            private set
            {
                if (value.Equals(uploadPercentage)) return;
                uploadPercentage = value;
                NotifyOfPropertyChange(() => UploadPercentage);
            }
        }

        public bool CanUploadToSkydrive { get { return selectedItems.Any(); } }

        private readonly IStorageFolder storageFolder;
        private bool isBusy;

        private readonly IAsyncXmlFileManager fileManager;
        private bool selectionEnabled;
        private readonly ISet<ResultFileViewModel> selectedItems;
        private readonly ISkyDriveUpload skyDriveUpload;
        private bool isUploading;
        private string uploadMessage;
        private double uploadPercentage;
        private bool isUsingLive;

        public BrowserPageViewModel(IStorageFolder appStorageFolder, 
            INavigationService navigationService, 
            IAsyncXmlFileManager xmlFileManager,
            ISkyDriveUpload skyDriveUpload) : base(navigationService)
        {
            ResultFiles = new BindableCollection<ResultFileViewModel>();
            selectedItems = new HashSet<ResultFileViewModel>();
            storageFolder = appStorageFolder;
            fileManager = xmlFileManager;
            this.skyDriveUpload = skyDriveUpload;
            this.skyDriveUpload.MessageChanged += (sender, args) => UploadMessage = args.Message;
            this.skyDriveUpload.UploadChanged += (sender, args) => IsUploading = args.IsUploading;
            this.skyDriveUpload.FileUploadFinished += (sender, args) => UploadPercentage = 0;
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
                ResultFiles.Add(new ResultFileViewModel
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
            
            if (result != MessageBoxResult.OK)
                return;

            foreach (var item in selectedItems.ToList())
            {
                ResultFiles.Remove(item);
                var file = await storageFolder.GetFileAsync(item.FileName);
                await file.DeleteAsync();

                try
                {
                    var svgFile = await storageFolder.GetFileAsync(AudiogramPathUtil.GetSvgFilePath(item.FileName));
                    await svgFile.DeleteAsync();
                }
                catch (FileNotFoundException)
                {
                }
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

            NotifyOfPropertyChange(() => CanUploadToSkydrive);
        }

        public void BackKeyPress(CancelEventArgs eventArgs)
        {
            if (SelectionEnabled)
            {
                SelectionEnabled = false;
                eventArgs.Cancel = true;
            }
        }

        public void Tapped(GestureEventArgs eventArgs)
        {
            if (SelectionEnabled)
            {
                eventArgs.Handled = true;
                return;
            }

            var viewModel = ((FrameworkElement) eventArgs.OriginalSource).DataContext as ResultFileViewModel;
            
            if (viewModel == null)
                return;

            eventArgs.Handled = true;
            NavigationService.UriFor<TestResultsPageViewModel>().WithParam(x => x.ResultFileName, viewModel.FileName).Navigate();
        }

        public async void UploadToSkydrive()
        {
            IsBusy = IsUsingLive = true;
            await skyDriveUpload.InitializeAsync();
            await skyDriveUpload.UploadAsync(selectedItems.Select(x => new SkyDriveFile(x.FileName, x.Description)).ToList(), this);
            IsBusy = IsUsingLive = false;
            var toast = new ToastPrompt
            {
                Title = "Upload completed",
                TextOrientation = Orientation.Horizontal,
                TextWrapping = TextWrapping.Wrap
            };
            toast.Show();
        }

        public void Report(LiveOperationProgress value)
        {
            UploadPercentage = value.ProgressPercentage;
        }
    }
}
