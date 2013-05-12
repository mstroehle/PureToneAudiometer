namespace PureToneAudiometer.ViewModels.Presets
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using Caliburn.Micro;

    public class AddItemViewModel : PropertyChangedBase, IDataErrorInfo
    {
        private string frequency;
        public string Frequency
        {
            get { return frequency; }
            set
            {
                if (value == frequency) return;
                frequency = value;
                NotifyOfPropertyChange(() => Frequency);
            }
        }

        public string ToneDuration
        {
            get { return toneDuration; }
            set
            {
                if (value == toneDuration) return;
                toneDuration = value;
                NotifyOfPropertyChange(() => ToneDuration);
            }
        }

        public string MinimumPauseDuration
        {
            get { return minimumPauseDuration; }
            set
            {
                if (value == minimumPauseDuration) return;
                minimumPauseDuration = value;
                NotifyOfPropertyChange(() => MinimumPauseDuration);
            }
        }

        public string MaximumPauseDuration
        {
            get { return maximumPauseDuration; }
            set
            {
                if (value == maximumPauseDuration) return;
                maximumPauseDuration = value;
                NotifyOfPropertyChange(() => MaximumPauseDuration);
            }
        }

        private readonly Dictionary<string, string> errorList = new Dictionary<string, string>(5);
        private string toneDuration;
        private string minimumPauseDuration;
        private string maximumPauseDuration;

        public string Error
        {
            get
            {
                if (errorList.Count > 1)
                {
                    var stringBuilder = new StringBuilder();
                    foreach (var item in errorList)
                    {
                        stringBuilder.Append("- ").AppendLine(item.Value);
                    }

                    return stringBuilder.ToString();
                }
                
                return errorList.FirstOrDefault().Value;
            }
        }

        public string this[string columnName]
        {
            get
            {
                var message = string.Empty;
                int integerTest;
                int minimumPause;
                int maximumPause;
                if(columnName == Property.Name(() => Frequency))
                {
                    if (string.IsNullOrEmpty(Frequency))
                    {
                        message = "Empty frequency value";
                    }
                    else if (!int.TryParse(Frequency, out integerTest))
                    {
                        message = "Invalid frequency value";
                    }
                }
                if(columnName == Property.Name(() => ToneDuration))
                {
                    if (string.IsNullOrEmpty(ToneDuration))
                    {
                        message = "Empty tone duration";
                    }
                    else if (!int.TryParse(ToneDuration, out integerTest))
                    {
                        message = "Invalid tone duration";
                    }
                }
                if (columnName == Property.Name(() => MinimumPauseDuration))
                {
                    if (string.IsNullOrEmpty(MinimumPauseDuration))
                        message = "Empty minimum pause duration";
                    else if (!int.TryParse(MinimumPauseDuration, out minimumPause))
                    {
                        message = "Invalid minimum pause duration";
                    }
                    else if (int.TryParse(MaximumPauseDuration, out maximumPause))
                    {
                        if (minimumPause >= maximumPause)
                        {
                            message = "Minimum pause can't be shorter than or equal to maximum pause";
                        }
                    }
                }
                if (columnName == Property.Name(() => MaximumPauseDuration))
                {
                    if (string.IsNullOrEmpty(MaximumPauseDuration))
                        message = "Empty maximum pause duration";
                    else if (!int.TryParse(MaximumPauseDuration, out maximumPause))
                    {
                        message = "Invalid maximum pause duration";
                    }
                    else if (int.TryParse(MinimumPauseDuration, out minimumPause))
                    {
                        if (maximumPause <= minimumPause)
                        {
                            message = "Maximum pause can't be shorter than or equal to minimum pause";
                        }
                    }
                }

                if (string.IsNullOrEmpty(message))
                    errorList.Remove(columnName);
                else
                    errorList[columnName] = message;

                return message;
            }
        }

        public AddItemViewModel()
        {
            ToneDuration = "1000";
            MinimumPauseDuration = "200";
            MaximumPauseDuration = "1000";
        }

        public PresetItemViewModel ToPresetItem()
        {
            return new PresetItemViewModel
                       {
                           Frequency = int.Parse(Frequency),
                           PitchDuration = int.Parse(ToneDuration),
                           MaximumPauseDuration = int.Parse(MaximumPauseDuration),
                           MinimumPauseDuration = int.Parse(MinimumPauseDuration)
                       };
        }
    }
}
