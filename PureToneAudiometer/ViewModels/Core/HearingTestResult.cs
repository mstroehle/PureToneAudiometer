namespace PureToneAudiometer.ViewModels.Core
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class HearingTestResult
    {
        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int MaxVolume { get; set; }

        [DataMember]
        public List<HearingResult> LeftChannel { get; set; }
        
        [DataMember]
        public List<HearingResult> RightChannel { get; set; }
       
        [DataMember]
        public bool IsFinished { get; set; }

        public HearingTestResult()
        {
            LeftChannel = new List<HearingResult>(30);
            RightChannel = new List<HearingResult>(30);
        }

        public HearingTestResult(IEnumerable<HearingResult> leftChannel, IEnumerable<HearingResult> rightChannel,
                          bool isFinished = true)
        {
            LeftChannel = new List<HearingResult>(leftChannel);
            RightChannel = new List<HearingResult>(rightChannel);
            IsFinished = isFinished;
        }
    }
}
