namespace PureToneAudiometer
{
    public static class Events
    {
        public class PresetItemsSelectionChanged
        {
            private readonly bool changedToSelectionScreen;
            public bool ChangedToSelectionScreen
            {
                get { return changedToSelectionScreen; }
            }

            public PresetItemsSelectionChanged(bool changedToSelectionScreen)
            {
                this.changedToSelectionScreen = changedToSelectionScreen;
            }
        }

        public class CanSavePreset
        {
            private readonly bool canSave;
            public bool CanSave { get { return canSave; } }

            public CanSavePreset(bool canSave)
            {
                this.canSave = canSave;
            }
        }

        public class PresetScheduledForDeletion
        {
            private readonly string fileName;

            public string FileName { get { return fileName; } }

            public PresetScheduledForDeletion(string fileName)
            {
                this.fileName = fileName;
            }
        }

        public class SelectNewPreset
        {
            private readonly string fileName;
            public string FileName { get { return fileName; } }

            public SelectNewPreset(string fileName)
            {
                this.fileName = fileName;
            }
        }

        public class AddItemValidationResult
        {
            private readonly bool passed;
            public bool Passed { get { return passed; } }

            public AddItemValidationResult(bool hasPassedValidation)
            {
                passed = hasPassedValidation;
            }
        }
    }
}
