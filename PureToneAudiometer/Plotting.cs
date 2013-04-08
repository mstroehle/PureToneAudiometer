namespace PureToneAudiometer
{
    using OxyPlot;
    using OxyPlot.Axes;

    public static class Plotting
    {
        public static PlotModel CreateLinearAxesModel(string title)
        {
            var model = new PlotModel(title);

            model.Axes.Add(new LinearAxis(AxisPosition.Bottom, "Frequency [Hz]"));
            model.Axes.Add(new LinearAxis(AxisPosition.Left, "Volume [dB HL]"));

            return model;
        }

        public static PlotModel CreateHalfLogarithmicAxesModel(string title)
        {
            var model = new PlotModel(title);

            model.Axes.Add(new LogarithmicAxis(AxisPosition.Bottom, "Frequency [Hz]"));
            model.Axes.Add(new LinearAxis(AxisPosition.Left, "Volume [dB HL]"));

            return model;
        }
    }
}
