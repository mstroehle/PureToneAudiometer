namespace PureToneAudiometer.Audio
{
    public interface IOscillator
    {
        int Attenuation { get; set; }
        int Frequency { get; set; }
        short GetValue();
        void Reset();
    }
}