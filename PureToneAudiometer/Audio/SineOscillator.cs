namespace PureToneAudiometer.Audio
{
    using System;
    using Resources;

    public class SineOscillator : IOscillator
    {
        private const double TwoPi = Math.PI*2;
      
        private const int TwoByteLimit = 65535;

        private int absoluteAttenuation = TwoByteLimit;
        private double frequency;
        private int attenuation;
        private uint currentAngle;
        private uint angleStep;

        private readonly int sampling;

        public SineOscillator(int attenuation, double frequency, int sampling)
        {
            this.sampling = sampling;
            Attenuation = attenuation;
            Frequency = frequency;
        }

        public SineOscillator(int attenuation, double frequency) : this(attenuation, frequency, 44100)
        {
        }

        public int Attenuation
        {
            get
            {
                return attenuation;
            }
            set
            {
                if(value > 0)
                    throw new ArgumentOutOfRangeException("value", value, AppResources.Oscillator_Attenuation_Exception);

                attenuation = value;
                absoluteAttenuation = (int)(TwoByteLimit * Math.Pow(10, Attenuation / 20.0));
            }
         
        }

        public double Frequency
        {
            get
            {
                return frequency;
            }
            set
            {
                frequency = value;
                angleStep = (uint)(frequency*uint.MaxValue/sampling);
            }
        }

        public short GetValue()
        {
            var amplitude = (int)(short.MaxValue  * Math.Sin(TwoPi * currentAngle / uint.MaxValue));
            var sample = (short)(amplitude * absoluteAttenuation >> 16);
        
            currentAngle += angleStep;

            return sample;
        }

        public void Reset()
        {
            currentAngle = 0;
        }
    }
}
