namespace PureToneAudiometer.Audio
{
    using System.Text;
    
    public static class Formatter
    {
        /// <summary>
        /// Converts the value to a little-endian base-16 string
        /// </summary>
        /// <param name="value"></param>
        /// <returns>A hex-string after little-endian conversion</returns>
        public static string ToLittleEndianBase16(int value)
        {
            //A little hack to easily create a little-endian value out of the big-endian int.
            return string.Format("{0:X8}", System.Net.IPAddress.HostToNetworkOrder(value));
        }

        /// <summary>
        /// Converts the value to a little-endian base-16 string
        /// </summary>
        /// <param name="value"></param>
        /// <returns>A hex-string after little-endian conversion</returns>
        public static string ToLittleEndianBase16(short value)
        {
            //A little hack to easily create a little-endian value out of the big-endian short.
            return string.Format("{0:X4}", System.Net.IPAddress.HostToNetworkOrder(value));
        } 

        /// <summary>
        /// Creates the PCM-format for <see cref="System.Windows.Media.MediaStreamAttributeKeys.CodecPrivateData"/>
        /// </summary>
        /// <param name="waveFormat">A <see cref="WaveFormat"/> instance</param>
        /// <returns>Little endian base16 encoded string</returns>
        public static string ToPcmBase16String(WaveFormat waveFormat)
        {
            var stringBuilder = new StringBuilder(100);
           
            stringBuilder
                .Append(ToLittleEndianBase16((short) waveFormat.Format))
                .Append(ToLittleEndianBase16((short) waveFormat.Channels))
                .Append(ToLittleEndianBase16(waveFormat.SamplesPerSecond))
                .Append(ToLittleEndianBase16(waveFormat.AverageBytesPerSecond))
                .Append(ToLittleEndianBase16(waveFormat.BlockAlignment))
                .Append(ToLittleEndianBase16((short) waveFormat.BitsPerSample))
                .Append(ToLittleEndianBase16(waveFormat.ExtraInfoSize));

            return stringBuilder.ToString();
        }
    }
}