namespace PureToneAudiometer.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Windows;
    using Caliburn.Micro;
    using Microsoft.Phone.Controls;
    using Presets;
    using Windows.Storage;

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

        public PresetsPageViewModel(IEventAggregator eventAggregator, PresetViewModel preset, SavedFilesViewModel savedFiles)
        {
            PresetViewModel = preset;
            SavedPresetsViewModel = savedFiles;
            eventAggregator.Subscribe(this);
            SelectIcon = new Uri("/Toolkit.Content/ApplicationBar.Select.png", UriKind.Relative);
            DeleteIcon = new Uri("/Toolkit.Content/ApplicationBar.Delete.png", UriKind.Relative);
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
            var localFolder = ApplicationData.Current.LocalFolder;

            try
            {
#pragma warning disable 168
                var existingFile = await localFolder.GetFileAsync(fileName);
#pragma warning restore 168
                
                var result = MessageBox.Show("There already is a preset with that name. Do you want to overwrite it?",
                                    "Saving preset", MessageBoxButton.OKCancel);

                if (result != MessageBoxResult.OK)
                {
                    return;
                }
            }
            catch (FileNotFoundException)
            {
            }

            var file = await localFolder.CreateFileAsync(fileName);
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                var serializer = new DataContractSerializer(typeof(List<PresetItemViewModel>));
                serializer.WriteObject(stream, PresetViewModel.PresetItems);
            }
        }

        public void Handle(Events.CanSavePreset message)
        {
            CanSaveItems = message.CanSave;
        }

        public async void Handle(Events.SelectNewPreset message)
        {
            var fullFileName = message.FileName + ".preset";
            var localFolder = ApplicationData.Current.LocalFolder;

            try
            {
                var localFile = await localFolder.GetFileAsync(fullFileName);
                
                using (var stream = await localFile.OpenStreamForReadAsync())
                {
                    var serializer = new DataContractSerializer(typeof (List<PresetItemViewModel>));
                    var presetItems = serializer.ReadObject(stream) as List<PresetItemViewModel>;

                    PresetViewModel.PresetItems.Clear();
                    PresetViewModel.PresetItems.AddRange(presetItems);
                    PresetViewModel.PresetName = message.FileName;
                    Index = 0;
                }
            }
            catch (FileNotFoundException)
            {
            }
        }
    }
}
