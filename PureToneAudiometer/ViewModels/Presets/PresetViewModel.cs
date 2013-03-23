namespace PureToneAudiometer.ViewModels.Presets
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Windows.Controls;
    using Caliburn.Micro;

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
            PresetItems = new BindableCollection<PresetItemViewModel>();
            SelectedItems = new BindableCollection<PresetItemViewModel>();

            Observable.FromEventPattern<PropertyChangedEventArgs>(this, "PropertyChanged")
                      .Where(x => x.EventArgs.PropertyName == "PresetName")
                      .Throttle(TimeSpan.FromMilliseconds(300))
                      .Subscribe(e =>
                                     {
                                         var canSaveFlag = !string.IsNullOrEmpty(PresetName);
                                         eventAggregator.Publish(new Events.CanSavePreset(canSaveFlag));
                                     });
        }

        public void AddNewItem()
        {
            PresetItems.Add(new PresetItemViewModel
                                {
                                    Frequency = 448
                                });                        
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
