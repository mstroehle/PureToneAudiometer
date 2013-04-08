namespace PureToneAudiometer.ViewModels.Core
{
    using System.Runtime.Serialization;

    [DataContract]
    public class HearingResult
    {
        [DataMember]
        public int Frequency { get; set; }
        [DataMember]
        public int Volume { get; set; }
    }
}
