namespace PureToneAudiometer.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Windows.Media;

    public class PureToneSource : MediaStreamSource
    {
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

        public PureToneSource(IPitchGenerator generator)
        {
            pitchGenerator = generator;
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
                var sample = pitchGenerator.GetSample();
                
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
