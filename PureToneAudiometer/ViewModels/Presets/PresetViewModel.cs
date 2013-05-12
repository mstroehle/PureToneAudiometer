namespace PureToneAudiometer.ViewModels.Presets
{
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using Caliburn.Micro;
    using Views.Presets;
    using Action = System.Action;

    public class PresetViewModel : ViewModelBase
    {
        private bool isSelectionEnabled;
        private IObservableCollection<PresetItemViewModel> selectedItems;
        public IObservableCollection<PresetItemViewModel> PresetItems
        {
            get { return presetItems; }
            private set
            {
                if (Equals(value, presetItems)) return;
                presetItems = value;
                NotifyOfPropertyChange(() => PresetItems);
            }
        }

        public IObservableCollection<PresetItemViewModel> SelectedItems
        {
            get { return selectedItems; }
            set
            {
                if (Equals(value, selectedItems)) return;
                selectedItems = value;
                NotifyOfPropertyChange(() => SelectedItems);
            }
        }

        public bool IsSelectionEnabled
        {
            get { return isSelectionEnabled; }
            set
            {
                if (value.Equals(isSelectionEnabled)) return;
                isSelectionEnabled = value;
                
                    
                NotifyOfPropertyChange(() => IsSelectionEnabled);
            }
        }

        public string PresetName
        {
            get { return presetName; }
            set
            {
                if (value == presetName) return;
                presetName = value;
                var canSaveFlag = !string.IsNullOrEmpty(PresetName) &&
                                  !PresetName.Any(x => invalidFileNameCharacters.Any(y => x == y));
                CanSave = canSaveFlag;
                
                NotifyOfPropertyChange(() => PresetName);
            }
        }

        private readonly char[] invalidFileNameCharacters = Path.GetInvalidFileNameChars();

        public bool CanSave
        {
            get { return canSave; }
            set
            {
                if (value.Equals(canSave)) return;
                canSave = value;
                NotifyOfPropertyChange(() => CanSave);
            }
        }

        private readonly IEventAggregator eventAggregator;

        private IObservableCollection<PresetItemViewModel> presetItems;
        private string presetName;
        private bool canSave;

        private readonly IDialogBuilder<AddItemView, AddItemViewModel> dialogBuilder;

        public PresetViewModel(INavigationService navigationService, IEventAggregator eventAggregator, IDialogBuilder<AddItemView, AddItemViewModel> builder) : base(navigationService)
        {
            this.eventAggregator = eventAggregator;
            dialogBuilder = builder;
            this.eventAggregator.Subscribe(this);
            PresetItems = new BindableCollection<PresetItemViewModel>();
            SelectedItems = new BindableCollection<PresetItemViewModel>();
        }

        public void AddNewItem()
        {
            dialogBuilder.Reset();
            var confirmationAction = new Action(() =>
                                                    {
                                                        var errorString = dialogBuilder.UnderlyingViewModel.Error;

                                                        if (!string.IsNullOrEmpty(errorString))
                                                            MessageBox.Show(errorString, "Validation error",
                                                                            MessageBoxButton.OK);
                                                        else
                                                        {
                                                            var newItem = dialogBuilder.UnderlyingViewModel.ToPresetItem();
                                                            var existingItem =
                                                                PresetItems.SingleOrDefault(
                                                                    x => x.Frequency == newItem.Frequency);
                                                            if (existingItem != null)
                                                                PresetItems.Remove(existingItem);

                                                            PresetItems.Add(newItem);
                                                            var items = PresetItems.OrderBy(x => x.Frequency).ToList();
                                                            PresetItems.Clear();
                                                            PresetItems.AddRange(items);
                                                        }
                                                    });
            dialogBuilder.Title("Add item...")
                         .LeftButtonContent("OK")
                         .RightButtonContent("Cancel")
                         .LeftButtonAction(confirmationAction)
                         .Show();            
        }

        public void SelectionChanged(SelectionChangedEventArgs args)
        {
            SelectedItems.AddRange(args.AddedItems.Cast<PresetItemViewModel>());
            SelectedItems.RemoveRange(args.RemovedItems.Cast<PresetItemViewModel>());

            if(args.AddedItems.Count == 0 && !SelectedItems.Any())
                eventAggregator.Publish(new Events.PresetItemsSelectionChanged(false));
            
            if(args.RemovedItems.Count == 0 && SelectedItems.Count == 1)
                eventAggregator.Publish(new Events.PresetItemsSelectionChanged(true));
        }

        public void DeleteSelectedItems()
        {
            foreach (var presetItemViewModel in SelectedItems.ToList())
            {
                PresetItems.Remove(presetItemViewModel);
            }
        }
    }
}
