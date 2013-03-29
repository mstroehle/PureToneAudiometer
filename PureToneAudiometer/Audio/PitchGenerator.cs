namespace PureToneAudiometer.Audio
{
    public enum Channel
    {
        None = 0,
        Right,
        Left,
        Both
    }

    public interface IPitchGenerator
    {
        int Attenuation { get; set; }
        int Frequency { get; set; }
        Channel MutedChannel { get; set; }
        Sample GetSample();
    }

    public class PitchGenerator : IPitchGenerator
    {
        private readonly IOscillator oscillator;
        private int attenuation;
        private int frequency;

        public int Attenuation
        {
            get { return attenuation; }
            set
            {
                attenuation = value;
                oscillator.Attenuation = value;
            }
        }


        public int Frequency
        {
            get { return frequency; }
            set
            {
                frequency = value;
                oscillator.Frequency = frequency;
            }
        }

        public Channel MutedChannel { get; set; }

        public PitchGenerator(IOscillator oscillator)
        {
            this.oscillator = oscillator;
        }

        public Sample GetSample()
        {
            var sampleValue = oscillator.GetValue();
            switch (MutedChannel)
            {
                case Channel.Both:
                    return new Sample(0, 0);
                case Channel.Right:
                    return new Sample(sampleValue, 0);
                case Channel.Left:
                    return new Sample(0, sampleValue);
                default:
                    return new Sample(sampleValue, sampleValue);
            }
        }
    }
}
