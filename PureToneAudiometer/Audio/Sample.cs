namespace PureToneAudiometer.Audio
{
    public struct Sample
    {
        private readonly short leftChannel;
        public short LeftChannel { get { return leftChannel; } }

        private readonly short rightChannel;
        public short RightChannel { get { return rightChannel; } }

        public Sample(short leftChannel, short rightChannel)
        {
            this.leftChannel = leftChannel;
            this.rightChannel = rightChannel;
        }
    }
}
