﻿namespace PureToneAudiometer.ViewModels
{
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using Caliburn.Micro;
    using Microsoft.Phone.Controls;
    using Presets;

    public sealed class PresetsPageViewModel :  Conductor<ViewModelBase>.Collection.OneActive, 
                                                IHandle<Events.PresetItemsSelectionChanged>, 
                                                IHandle<Events.SelectNewPreset>
    {
        public PresetViewModel PresetViewModel { get; private set; }
        public SavedFilesViewModel SavedPresetsViewModel { get; private set; }

        private bool isAppBarVisible;
        private int index;


        public bool IsAppBarVisible
        {
            get { return isAppBarVisible; }
            set
            {
                if (value.Equals(isAppBarVisible)) return;
                isAppBarVisible = value;
                NotifyOfPropertyChange(() => IsAppBarVisible);
            }
        }

        public int Index
        {
            get { return index; }
            set
            {
                if (value == index) return;
                index = value;
                NotifyOfPropertyChange(() => Index);
            }
        }

        private readonly IAsyncXmlFileManager manager;
        private readonly IEventAggregator eventAggregator;
        public PresetsPageViewModel(IEventAggregator eventAggregator, IAsyncXmlFileManager manager, PresetViewModel preset, SavedFilesViewModel savedFiles)
        {
            this.eventAggregator = eventAggregator;
            PresetViewModel = preset;
            SavedPresetsViewModel = savedFiles;
            this.manager = manager;
            IsAppBarVisible = true;
            
            ActivateItem(PresetViewModel);
        }

        protected override void OnActivate()
        {
            eventAggregator.Subscribe(this);
        }

        protected override void OnDeactivate(bool close)
        {
            eventAggregator.Unsubscribe(this);
        }

        public void SelectItems()
        {
            PresetViewModel.IsSelectionEnabled = true;
        }

        public void DeleteItems()
        {
            PresetViewModel.DeleteSelectedItems();
            PresetViewModel.IsSelectionEnabled = false;
        }

        public void LoadedPivotItem(PivotItemEventArgs eventArgs)
        {
            if(eventArgs.Item.Name == "PresetViewModel")
            {
                IsAppBarVisible = true;
                ActivateItem(PresetViewModel);
            }
            else
            {
                IsAppBarVisible = false;
                ActivateItem(SavedPresetsViewModel);
            }
        }

        public void AddNewItem()
        {
            PresetViewModel.AddNewItem();
        }

        public void BackKeyPressed(CancelEventArgs eventArgs)
        {
            if (PresetViewModel.IsSelectionEnabled)
            {
                PresetViewModel.IsSelectionEnabled = false;
                eventArgs.Cancel = true;
            }
        }

        public void Handle(Events.PresetItemsSelectionChanged message)
        {
            if (message.ChangedToSelectionScreen)
                SelectItems();
            else
            {
                PresetViewModel.IsSelectionEnabled = false;
            }
        }

        public async void SaveItems()
        {
            var fileName = PresetViewModel.PresetName + ".preset";
            manager.FileName = fileName;

            var items = (await manager.GetCollection<PresetItemViewModel>()).ToList();
            if (items.Any())
            {
                var result = MessageBox.Show("There already is a preset with that name. Do you want to overwrite it?",
                                   "Saving preset", MessageBoxButton.OKCancel);

                if (result != MessageBoxResult.OK)
                {
                    return;
                }
            }

            await manager.Save(PresetViewModel.PresetItems.ToList());
        }

        public void NewPreset()
        {
            PresetViewModel.IsSelectionEnabled = false;
            PresetViewModel.PresetName = null;
            PresetViewModel.PresetItems.Clear();
        }

        public async void Handle(Events.SelectNewPreset message)
        {
            var fullFileName = message.FileName + ".preset";
            manager.FileName = fullFileName;
            
            var items = await manager.GetCollection<PresetItemViewModel>();

            PresetViewModel.PresetItems.Clear();
            PresetViewModel.PresetItems.AddRange(items);
            PresetViewModel.PresetName = message.FileName;
            Index = 0;
        }
    }
}
