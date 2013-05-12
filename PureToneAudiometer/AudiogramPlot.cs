namespace PureToneAudiometer
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using OxyPlot;
    using OxyPlot.Axes;
    using OxyPlot.Series;
    using OxyPlot.Silverlight;
    using ViewModels.Core;
    using Windows.Storage;

    public interface ICreateAudiogram
    {
        PlotModel PlotModel { get; }
        TestResult TestResult { get; }
        Task CreateFromAsync(string plotDataFilePath);
        string LastUsedPath { get; }
    }

    public interface ISvgExport
    {
        Task SaveToFileAsync();
    }

    public interface IAudiogramPlot : ICreateAudiogram, ISvgExport
    {
        void Update();
        Task SaveAsync();
        Task SaveAllAndUpdateAsync();
    }

    public class AudiogramPlot : IAudiogramPlot
    {
        private readonly IStorageFolder storageFolder;
        private readonly IAsyncXmlFileManager xmlFileManager;

        public AudiogramPlot(IStorageFolder folder, IAsyncXmlFileManager xmlFileManager)
        {
            storageFolder = folder;
            this.xmlFileManager = xmlFileManager;
        }

        public async Task SaveToFileAsync()
        {
            var svgPath = AudiogramPathUtil.GetSvgFilePath(LastUsedPath);
            var svg = SvgExporter.ExportToString(PlotModel, 1920, 1280, true,
                                                 new SilverlightRenderContext(new InkPresenter()));

            var file = await storageFolder.CreateFileAsync(svgPath, CreationCollisionOption.ReplaceExisting);
            
            using (var stream = await file.OpenStreamForWriteAsync())
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteAsync(svg);
            }
        }

        public PlotModel PlotModel { get; private set; }
        public TestResult TestResult { get; private set; }

        public async Task CreateFromAsync(string plotDataFilePath)
        {
            LastUsedPath = plotDataFilePath;
            xmlFileManager.FileName = plotDataFilePath;
            
            TestResult = await xmlFileManager.Get<TestResult>();

            Update();
        }

        public string LastUsedPath { get; private set; }

        public void Update()
        {
            var model = new PlotModel(TestResult.Description);
            
            model.Axes.Add(new LinearAxis(AxisPosition.Top, "Frequency [Hz]")
                               {
                                   MajorGridlineStyle = LineStyle.Solid,
                                   MinorGridlineStyle = LineStyle.Dot
                               });
            var axis = TestResult.MaxVolume != default(int)
                                  ? new LinearAxis(AxisPosition.Left, 0, TestResult.MaxVolume, "Volume [dB HL]")
                                        {
                                            MajorGridlineStyle = LineStyle.Solid,
                                            MinorGridlineStyle = LineStyle.Dot
                                        }
                                  : new LinearAxis(AxisPosition.Left, "Volume [dB HL]")
                                        {
                                            MajorGridlineStyle = LineStyle.Solid,
                                            MinorGridlineStyle = LineStyle.Dot
                                        };
            axis.EndPosition = 0;
            axis.StartPosition = 1;
            model.Axes.Add(axis);

            var leftChannelSeries = new LineSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 6,
                MarkerStroke = OxyColors.Red,
                MarkerFill = OxyColors.Red,
                Color = OxyColors.Red
            };
            var rightChannelSeries = new LineSeries
            {
                MarkerType = MarkerType.Cross,
                MarkerSize = 6,
                MarkerStroke = OxyColors.Blue,
                MarkerFill = OxyColors.Blue,
                Color = OxyColors.Blue
            };

            foreach (var item in TestResult.LeftChannel)
            {
                leftChannelSeries.Points.Add(new DataPoint(item.Frequency, item.Volume));
            }

            foreach (var item in TestResult.RightChannel)
            {
                rightChannelSeries.Points.Add(new DataPoint(item.Frequency, item.Volume));
            }

            model.Series.Add(leftChannelSeries);
            model.Series.Add(rightChannelSeries);

            PlotModel = model;
        }

        public async Task SaveAsync()
        {
            await xmlFileManager.Save(TestResult);
        }

        public async Task SaveAllAndUpdateAsync()
        {
            await SaveAsync();
            await SaveToFileAsync();
            Update();
        }
    }
}
