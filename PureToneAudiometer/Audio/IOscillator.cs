namespace PureToneAudiometer.Audio
{
    public interface IOscillator
    {
        int Attenuation { get; set; }
        double Frequency { get; set; }
        short GetValue();
        void Reset();
    }
}