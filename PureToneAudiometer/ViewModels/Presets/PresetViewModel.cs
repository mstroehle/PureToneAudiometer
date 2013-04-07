namespace PureToneAudiometer.ViewModels.Presets
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using Caliburn.Micro;
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
                IsEnforceSelectionEnabled = !isSelectionEnabled;
            }
        }

        public string PresetName
        {
            get { return presetName; }
            set
            {
                if (value == presetName) return;
                presetName = value;

                
                NotifyOfPropertyChange(() => PresetName);
            }
        }

        public bool IsEnforceSelectionEnabled
        {
            get { return isEnforceSelectionEnabled; }
            set
            {
                if (value.Equals(isEnforceSelectionEnabled)) return;
                isEnforceSelectionEnabled = value;
                NotifyOfPropertyChange(() => IsEnforceSelectionEnabled);
            }
        }

        private readonly IEventAggregator eventAggregator;

        private bool isEnforceSelectionEnabled;
        private IObservableCollection<PresetItemViewModel> presetItems;
        private string presetName;

        public PresetViewModel(INavigationService navigationService, IEventAggregator eventAggregator) : base(navigationService)
        {
            this.eventAggregator = eventAggregator;

            this.eventAggregator.Subscribe(this);
            PresetItems = new BindableCollection<PresetItemViewModel>();
            SelectedItems = new BindableCollection<PresetItemViewModel>();

            Observable.FromEventPattern<PropertyChangedEventArgs>(this, "PropertyChanged")
                      .Where(x => x.EventArgs.PropertyName == "PresetName")
                      .Throttle(TimeSpan.FromMilliseconds(300))
                      .Subscribe(e =>
                                     {
                                         var invalidFileNameCharacters = Path.GetInvalidFileNameChars();
                                         var canSaveFlag = !string.IsNullOrEmpty(PresetName) &&
                                                           !PresetName.Any(x => invalidFileNameCharacters.Any(y => x == y));
                                         eventAggregator.Publish(new Events.CanSavePreset(canSaveFlag));
                                     });
        }

        public void AddNewItem()
        {
            var builder = new AddItemDialogBuilder();

            var confirmationAction = new Action(() =>
                                                    {
                                                        var errorString = builder.UnderlyingViewModel.Error;

                                                        if (!string.IsNullOrEmpty(errorString))
                                                            MessageBox.Show(errorString, "Validation error",
                                                                            MessageBoxButton.OK);
                                                        else
                                                        {
                                                            var newItem = builder.UnderlyingViewModel.ToPresetItem();
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
            builder.Title("Add item...")
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
