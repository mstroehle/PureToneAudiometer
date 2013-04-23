namespace PureToneAudiometer.ViewModels.Results
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using Caliburn.Micro;

    public sealed class SaveResultViewModel : PropertyChangedBase, IDataErrorInfo
    {
        private readonly Dictionary<string, string> errorList = new Dictionary<string, string>(5);

        private string description;
        public string Description
        {
            get { return description; }
            set
            {
                if (value == description) return;
                description = value;
                NotifyOfPropertyChange(() => Description);
            }
        }

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
                switch (columnName)
                {
                    case "Description":
                        if (string.IsNullOrEmpty(Description))
                            message = "Description can't be empty";
                        break;
                }

                if (string.IsNullOrEmpty(message))
                    errorList.Remove(columnName);
                else
                    errorList[columnName] = message;

                return message;
            }
        }
    }
}
