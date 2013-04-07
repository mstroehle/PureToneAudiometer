namespace PureToneAudiometer.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using Caliburn.Micro;
    using Microsoft.Phone.Controls;
    using Presets;

    public sealed class PresetsPageViewModel :  Conductor<ViewModelBase>.Collection.OneActive, 
                                                IHandle<Events.PresetItemsSelectionChanged>, 
                                                IHandle<Events.CanSavePreset>,
                                                IHandle<Events.SelectNewPreset>
    {
        public PresetViewModel PresetViewModel { get; private set; }
        public SavedFilesViewModel SavedPresetsViewModel { get; private set; }

        private Uri selectIcon;
        private Uri deleteIcon;
        private bool isSelectVisible;
        private bool isAppBarVisible;
        private bool saveItems;
        private int index;

        public Uri SelectIcon
        {
            get { return selectIcon; }
            private set
            {
                if (Equals(value, selectIcon)) return;
                selectIcon = value;
                NotifyOfPropertyChange(() => SelectIcon);
            }
        }

        public Uri DeleteIcon
        {
            get { return deleteIcon; }
            set
            {
                if (Equals(value, deleteIcon)) return;
                deleteIcon = value;
                NotifyOfPropertyChange(() => DeleteIcon);
            }
        }

        public Uri SaveIcon
        {
            get { return saveIcon; }
            set
            {
                if (Equals(value, saveIcon)) return;
                saveIcon = value;
                NotifyOfPropertyChange(() => SaveIcon);
            }
        }

        public bool IsSelectVisible
        {
            get { return isSelectVisible; }
            set
            {
                if (value.Equals(isSelectVisible)) return;
                isSelectVisible = value;
                NotifyOfPropertyChange(() => IsSelectVisible);
            }
        }

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

        public bool CanSaveItems
        {
            get { return saveItems; }
            set
            {
                if (value.Equals(saveItems)) return;
                saveItems = value;
                NotifyOfPropertyChange(() => CanSaveItems);
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

        private Uri saveIcon;

        private readonly IXmlItemsFileManager<PresetItemViewModel> itemsManager;

        public PresetsPageViewModel(IEventAggregator eventAggregator, IXmlItemsFileManager<PresetItemViewModel> itemsManager, PresetViewModel preset, SavedFilesViewModel savedFiles)
        {
            PresetViewModel = preset;
            SavedPresetsViewModel = savedFiles;
            this.itemsManager = itemsManager;
            eventAggregator.Subscribe(this);
            SelectIcon = new Uri("/Toolkit.Content/ApplicationBar.Select.png", UriKind.Relative);
            DeleteIcon = new Uri("/Toolkit.Content/ApplicationBar.Delete.png", UriKind.Relative);
            SaveIcon = new Uri("/Assets/SaveIcon.png", UriKind.Relative);
            IsSelectVisible = true;
            IsAppBarVisible = true;
            
            ActivateItem(PresetViewModel);
        }

        public void SelectItems()
        {
            IsSelectVisible = false;
            PresetViewModel.IsSelectionEnabled = false;
        }

        public void DeleteItems()
        {
            PresetViewModel.DeleteSelectedItems();
            SelectionEnded();
        }

        private void SelectionEnded()
        {
            PresetViewModel.IsSelectionEnabled = true;
            IsSelectVisible = true;
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

        public void BackKeyPressed(CancelEventArgs eventArgs)
        {
            if (PresetViewModel.IsSelectionEnabled)
            {
                SelectionEnded();
                eventArgs.Cancel = true;
            }
        }

        public void Handle(Events.PresetItemsSelectionChanged message)
        {
            if (message.ChangedToSelectionScreen)
                SelectItems();
            else
            {
                SelectionEnded();
            }
        }

        public async void SaveItems()
        {
            var fileName = PresetViewModel.PresetName + ".preset";
            itemsManager.FileName = fileName;

            var items = (await itemsManager.GetAsync()).ToList();
            if (items.Any())
            {
                var result = MessageBox.Show("There already is a preset with that name. Do you want to overwrite it?",
                                   "Saving preset", MessageBoxButton.OKCancel);

                if (result != MessageBoxResult.OK)
                {
                    return;
                }
            }

            await itemsManager.Save(PresetViewModel.PresetItems);
        }

        public void NewPreset()
        {
            PresetViewModel.IsSelectionEnabled = false;
            PresetViewModel.PresetName = null;
            PresetViewModel.PresetItems.Clear();
        }

        public void Handle(Events.CanSavePreset message)
        {
            CanSaveItems = message.CanSave;
        }

        public async void Handle(Events.SelectNewPreset message)
        {
            var fullFileName = message.FileName + ".preset";
            itemsManager.FileName = fullFileName;
            
            var items = await itemsManager.GetAsync();

            PresetViewModel.PresetItems.Clear();
            PresetViewModel.PresetItems.AddRange(items);
            PresetViewModel.PresetName = message.FileName;
            Index = 0;
        }
    }
}
