namespace PureToneAudiometer.ViewModels.Core
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class HearingResult
    {
        [DataMember]
        public int Frequency { get; set; }
        [DataMember]
        public int Volume { get; set; }
    }

    public class FrequencyResultComparer : IEqualityComparer<HearingResult>
    {
        public bool Equals(HearingResult x, HearingResult y)
        {
            return x.Frequency == y.Frequency;
        }

        public int GetHashCode(HearingResult obj)
        {
            return obj.Frequency.GetHashCode();
        }
    }
}
