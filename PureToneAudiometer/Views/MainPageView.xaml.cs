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
            pitchGenerator = new PitchGenerator(new SineOscillator(-15, 100));
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
                Media.SetSource(new PureToneSource(pitchGenerator, TimeSpan.FromSeconds(1), new PauseDuration(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(2))));
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
            pitchGenerator.MutedChannel = Channel.Right;
            Media.Balance = -1.0;
        }

        private void BothButton_OnClick(object sender, RoutedEventArgs e)
        {
            pitchGenerator.MutedChannel = Channel.None;
            Media.Balance = 0;
        }

        private void RightButton_OnClick(object sender, RoutedEventArgs e)
        {
            pitchGenerator.MutedChannel = Channel.Left;
            Media.Balance = 1.0;
        }

        private void Balance_OnChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Media.Balance = e.NewValue*0.2 - 1;
        }
    }
}