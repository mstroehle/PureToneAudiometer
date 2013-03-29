namespace PureToneAudiometer.ViewModels.Presets
{
    using System.Runtime.Serialization;

    [DataContract]
    public class PresetItemViewModel
    {
        [DataMember]
        public int Frequency { get; set; }
        [DataMember]
        public int PitchDuration { get; set; }
        [DataMember]
        public int MinimumPauseDuration { get; set; }
        [DataMember]
        public int MaximumPauseDuration { get; set; }
    }
}
