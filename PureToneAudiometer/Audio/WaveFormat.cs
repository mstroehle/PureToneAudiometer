namespace PureToneAudiometer.Audio
{
    /// <summary>
    /// Waveform-audio format types. Only one is required for the application to work - Pulse-code modulated waveform
    /// </summary>
    public enum FormatTag : short
    {
        PCM = 1
    }

    /// <summary>
    /// Number of channels
    /// </summary>
    public enum Channels
    {
        Mono = 1,
        Stereo = 2
    }

    public enum BitsPerSample : short
    {
        PcmMono = 8,
        PcmStereo = 16
    }

    /// <summary>
    /// Wave format for the audio codec using the WAVEFORMATEX structure
    /// http://msdn.microsoft.com/en-us/library/dd390970%28v=vs.85%29.aspx
    /// </summary>
    public class WaveFormat
    {
        public FormatTag Format { get; private set; }
        public Channels Channels { get; private set; }
        public int SamplesPerSecond { get; private set; }
        public int AverageBytesPerSecond { get; private set; }
        public short BlockAlignment { get; private set; }
        public BitsPerSample BitsPerSample { get; private set; }
        public short ExtraInfoSize { get; private set; }

        public class Builder
        {
            private FormatTag format;
            private Channels channels;
            private int samplesPerSecond;
            private int averageBytesPerSecond;
            private short blockAlignment;
            private BitsPerSample bitsPerSample;
            private short extraInfoSize;

            public Builder Format(FormatTag value)
            {
                format = value;
                return this;
            }

            public Builder Channels(Channels value)
            {
                channels = value;
                return this;
            }

            public Builder SamplesPerSecond(int value)
            {
                samplesPerSecond = value;
                return this;
            }

            public Builder AverageBytesPerSecond(int value)
            {
                averageBytesPerSecond = value;
                return this;
            }

            public Builder BlockAlignment(short value)
            {
                blockAlignment = value;
                return this;
            }

            public Builder BitsPerSample(BitsPerSample value)
            {
                bitsPerSample = value;
                return this;
            }

            public Builder ExtraInfoSize(short value)
            {
                extraInfoSize = 0;
                return this;
            }

            public WaveFormat Build()
            {
                return new WaveFormat
                           {
                               Format = format,
                               AverageBytesPerSecond = averageBytesPerSecond,
                               BitsPerSample = bitsPerSample,
                               BlockAlignment = blockAlignment,
                               Channels = channels,
                               ExtraInfoSize = extraInfoSize,
                               SamplesPerSecond = samplesPerSecond
                           };
            }
        }
    }
}