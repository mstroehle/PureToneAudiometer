namespace PureToneAudiometer.ViewModels.Start
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class RecentItemViewModel
    {
        [DataMember]
        public string PresetName { get; set; }

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public DateTime LastUsedDate { get; set; }

        public string LastUsedMessage
        {
            get { return "Last used: " + LastUsedDate.ToString("yyyy/MM/dd HH:mm"); }
        }
    }
}
