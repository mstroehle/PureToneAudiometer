﻿namespace PureToneAudiometer.ViewModels.Results
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Caliburn.Micro;
    using Core;
    using Windows.Storage;
    using System;

    public class BrowserPageViewModel : ViewModelBase
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
        public BrowserPageViewModel(IStorageFolder appStorageFolder, 
            INavigationService navigationService, 
            IAsyncXmlFileManager xmlFileManager) : base(navigationService)
        {
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

        public void GoToLiveApi()
        {
            NavigationService.UriFor<Live.MainLivePageViewModel>().WithParam(x => x.CombinedPath, string.Join(";", selectedItems.Select(x => x.FileName))).Navigate();
        }
    }
}
