namespace PureToneAudiometer.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using System.Windows.Threading;

    public struct PauseDuration
    {
        private readonly TimeSpan minimum;

        public TimeSpan Minimum
        {
            get { return minimum; }
        }

        private readonly TimeSpan maximum;

        public TimeSpan Maximum
        {
            get { return maximum; }
        }

        public PauseDuration(TimeSpan min, TimeSpan max)
        {
            minimum = min;
            maximum = max;
        }
    }

    public class PureToneSource : MediaStreamSource
    {
        private struct PauseDurationInternal
        {
            private readonly int minimumMillis;

            public int MinimumMillis
            {
                get { return minimumMillis; }
            }

            private readonly int maximumMillis;

            public int MaximumMillis
            {
                get { return maximumMillis; }
            }

            private PauseDurationInternal(TimeSpan min, TimeSpan max)
            {
                minimumMillis = (int)min.TotalMilliseconds;
                maximumMillis = (int)max.TotalMilliseconds;
            }

            public static PauseDurationInternal From(PauseDuration duration)
            {
                return new PauseDurationInternal(duration.Minimum, duration.Maximum);
            }

            public static implicit operator PauseDurationInternal(PauseDuration duration)
            {
                return PauseDurationInternal.From(duration);
            }
        }

        private static readonly Random random = new Random();

        public const int SamplesPerSecond = 44100;

        private const Channels SelectedChannelsSetup = Channels.Stereo;
        private const BitsPerSample SelectedBitsPerSample = BitsPerSample.PcmStereo;
        private const int BytesPerSample = (short)SelectedBitsPerSample/8;

        private const int NumberOfSamples = 64;

        private const int BufferByteLength = (int) SelectedChannelsSetup*BytesPerSample*NumberOfSamples;

        private MemoryStream memoryStream;
        private BinaryWriter binaryWriter;

        private long currentPosition;
        private long currentTimeStamp;

        private static readonly WaveFormat DefaultWaveFormat;
        private MediaStreamDescription streamDescription;

        private static readonly IDictionary<MediaSampleAttributeKeys, string> OtherAttributes =
            new ReadOnlyDictionary<MediaSampleAttributeKeys, string>(new Dictionary<MediaSampleAttributeKeys, string>()); 

        private readonly IPitchGenerator pitchGenerator;
        
        private readonly DispatcherTimer timer = new DispatcherTimer();

        private readonly PauseDurationInternal pauseDuration;
        

        private bool muteSamples = false;

        static PureToneSource()
        {
            var waveFormatBuilder = new WaveFormat.Builder();
            waveFormatBuilder
                .Format(FormatTag.PCM)
                .Channels(SelectedChannelsSetup)
                .SamplesPerSecond(SamplesPerSecond)
                .AverageBytesPerSecond(SamplesPerSecond * (int)SelectedChannelsSetup * BytesPerSample)
                .BlockAlignment((int)SelectedChannelsSetup * BytesPerSample)
                .BitsPerSample(SelectedBitsPerSample)
                .ExtraInfoSize(0);

            DefaultWaveFormat = waveFormatBuilder.Build();
        }

        public PureToneSource(IPitchGenerator generator, TimeSpan sampleLength, PauseDuration pauseDuration)
        {
            pitchGenerator = generator;
            timer.Interval = sampleLength;
            timer.Tick += TimerOnTick;
            this.pauseDuration = pauseDuration;
        }

        private async void TimerOnTick(object sender, EventArgs eventArgs)
        {
            timer.Stop();
            muteSamples = true;
            await Task.Delay(random.Next(pauseDuration.MinimumMillis, pauseDuration.MaximumMillis));
            muteSamples = false;
            timer.Start();
        }

        protected override void OpenMediaAsync()
        {
            currentPosition = currentTimeStamp = 0;
            memoryStream = new MemoryStream();
            binaryWriter = new BinaryWriter(memoryStream);

            var mediaStreamAttributes = new Dictionary<MediaStreamAttributeKeys, string>(1);
            var mediaSourceAttributes = new Dictionary<MediaSourceAttributesKeys, string>(3);

            mediaStreamAttributes[MediaStreamAttributeKeys.CodecPrivateData] = Formatter.ToPcmBase16String(DefaultWaveFormat);
            mediaSourceAttributes[MediaSourceAttributesKeys.CanSeek] = "false";
            mediaSourceAttributes[MediaSourceAttributesKeys.Duration] = "0";

            streamDescription = new MediaStreamDescription(MediaStreamType.Audio, mediaStreamAttributes);

            timer.Start();

            ReportOpenMediaCompleted(mediaSourceAttributes, new[] { streamDescription });
        }

        protected override void SeekAsync(long seekToTime)
        {
            ReportSeekCompleted(seekToTime);
        }

        protected override void GetSampleAsync(MediaStreamType mediaStreamType)
        {
          
            for (var i = 0; i < NumberOfSamples; i++)
            {
                var sample = muteSamples ? new Sample() : pitchGenerator.GetSample();

                binaryWriter.Write(sample.LeftChannel);
                binaryWriter.Write(sample.RightChannel);
            }

            var mediaStreamSample = new MediaStreamSample(streamDescription, binaryWriter.BaseStream, currentPosition,
                                                          BufferByteLength, 0, OtherAttributes);

            currentPosition += BufferByteLength;

            ReportGetSampleCompleted(mediaStreamSample);
        }

        protected override void SwitchMediaStreamAsync(MediaStreamDescription mediaStreamDescription)
        {
            throw new NotImplementedException();
        }

        protected override void GetDiagnosticAsync(MediaStreamSourceDiagnosticKind diagnosticKind)
        {
            throw new NotImplementedException();
        }

        protected override void CloseMedia()
        {
            binaryWriter.Dispose();
            streamDescription = null;
        }
    }
}
