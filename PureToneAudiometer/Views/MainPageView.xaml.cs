namespace PureToneAudiometer.Views
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using Audio;

    public partial class MainPageView
    {
        private readonly IPitchGenerator pitchGenerator;
        private bool playing;        

        // Constructor
        public MainPageView()
        {
            InitializeComponent();
            pitchGenerator = new PitchGenerator(new Oscillator(-15, 100));
        }

        private void PlayTone_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            if (this.playing)
            {
                this.playing = false;
                button.Content = "Play";
                this.Media.Stop();
            }
            else
            {
                this.playing = true;
                button.Content = "Stop";
                Media.SetSource(new PureToneSource(pitchGenerator));
            }
        }

        private void Attenuation_OnChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var newAttenuation = Convert.ToInt32(e.NewValue*5 - 50);
            pitchGenerator.Attenuation = newAttenuation;
        }

        private void Frequency_OnChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var newFrequency = e.NewValue*800;
            pitchGenerator.Frequency = newFrequency;
        }

        private void LeftButton_OnClick(object sender, RoutedEventArgs e)
        {
            Media.Balance = -1;
        }

        private void RightButton_OnClick(object sender, RoutedEventArgs e)
        {
            Media.Balance = 1;
        }
    }
}