namespace PureToneAudiometer.ViewModels.Results
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using Caliburn.Micro;
    using Microsoft.Phone.Controls;
    using Views.Results;

    public class ResultsPageViewModel : Conductor<ViewModelBase>.Collection.OneActive
    {
        public string ResultFileName { get; set; }

        public PlotViewModel PlotViewModel
        {
            get { return plotViewModel; }
            private set
            {
                if (Equals(value, plotViewModel)) return;
                plotViewModel = value;
                NotifyOfPropertyChange(() => PlotViewModel);
            }
        }

        public DataViewModel DataViewModel
        {
            get { return dataViewModel; }
            private set
            {
                if (Equals(value, dataViewModel)) return;
                dataViewModel = value;
                NotifyOfPropertyChange(() => DataViewModel);
            }
        }

        public bool IsAppBarVisible
        {
            get { return isAppBarVisible; }
            private set
            {
                if (value.Equals(isAppBarVisible)) return;
                isAppBarVisible = value;
                NotifyOfPropertyChange(() => IsAppBarVisible);
            }
        }

        private readonly IDialogBuilder<SaveResultView, SaveResultViewModel> dialogBuilder;
        private PlotViewModel plotViewModel;
        private DataViewModel dataViewModel;
        private bool isAppBarVisible;
        private readonly INavigationService navigationService;

        public ResultsPageViewModel(INavigationService navigationService, 
            IDialogBuilder<SaveResultView, SaveResultViewModel> builder,
            PlotViewModel plotViewModel,
            DataViewModel dataViewModel)
        {
            this.navigationService = navigationService;
            PlotViewModel = plotViewModel;
            DataViewModel = dataViewModel;
            dialogBuilder = builder;
        }

        protected override void OnActivate()
        {
            PlotViewModel.ResultFileName = ResultFileName;
            DataViewModel.ResultFileName = ResultFileName;

            ActivateItem(PlotViewModel);
            IsAppBarVisible = true;
        }

        public void LoadedPivotItem(PivotItemEventArgs eventArgs)
        {
            if (eventArgs.Item.Name == "PlotViewModel")
            {
                IsAppBarVisible = true;
                ActivateItem(PlotViewModel);
            }
            else
            {
                IsAppBarVisible = false;
                ActivateItem(DataViewModel);
            }
        }

        public void SaveResult()
        {
            dialogBuilder.Reset();
            var confirmationAction = new System.Action(async () =>
                                                           {
                                                               var errorString = dialogBuilder.UnderlyingViewModel.Error;

                                                               if (!string.IsNullOrEmpty(errorString))
                                                                   MessageBox.Show(errorString, "Validation error",
                                                                                   MessageBoxButton.OK);
                                                               else
                                                               {
                                                                   var description =
                                                                       dialogBuilder.UnderlyingViewModel.Description;
                                                                   PlotViewModel.Description = description;
                                                                   await PlotViewModel.UpdateAndSaveAsync();
                                                               }
                                                           });
            dialogBuilder.Title("Enter description")
                         .LeftButtonContent("OK")
                         .RightButtonContent("Cancel")
                         .LeftButtonAction(confirmationAction);

            dialogBuilder.Show();
        }

        private async Task<bool> Discard()
        {
            if (string.IsNullOrEmpty(PlotViewModel.Description))
            {
                var result = MessageBox.Show("Are you sure you want to discard the result?", "Confirmation",
                                MessageBoxButton.OKCancel);

                if (result != MessageBoxResult.OK)
                    return false;

                await PlotViewModel.DeleteAll();
                return true;
            }

            return true;
        }

        public async void GoToMainMenu()
        {
            var result = await Discard();

            if (!result)
                return;

            navigationService.UriFor<MainPageViewModel>().Navigate();
        }

        public async void BackKeyPress()
        {
            if (string.IsNullOrEmpty(PlotViewModel.Description))
            {
                await PlotViewModel.DeleteAll();
            }
        }
    }
}
