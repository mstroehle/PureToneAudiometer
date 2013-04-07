namespace PureToneAudiometer
{
    using Audio;
    using ViewModels.Presets;

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

        public class UsePreset
        {
            private readonly string fileName;
            public string FileName { get { return fileName; } }

            public UsePreset(string fileName)
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

        public static class HearingTest
        {
            public class StartPlaying
            {
                private readonly PresetItemViewModel presetItem;

                public PresetItemViewModel Preset { get { return presetItem; } }

                public StartPlaying(PresetItemViewModel preset)
                {
                    presetItem = preset;
                }
            }

            public class StopPlaying
            {
            }

            public class Deactivate
            {
            }

            public class PitchGeneratorChanged
            {
                private readonly IPitchGenerator pitchGenerator;

                public IPitchGenerator PitchGenerator { get { return pitchGenerator; } }

                public PitchGeneratorChanged(IPitchGenerator newGenerator)
                {
                    pitchGenerator = newGenerator;
                }
            }

            public class ChannelChanged
            {
                private readonly Channel newActiveChannel;

                public Channel NewActiveChannel { get { return newActiveChannel; } }

                public ChannelChanged(Channel newChannel)
                {
                    newActiveChannel = newChannel;
                }
            }
        }
    }
}
