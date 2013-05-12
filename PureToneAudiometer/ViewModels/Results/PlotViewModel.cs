
namespace PureToneAudiometer.ViewModels.Results
{
    using System.IO;
    using System.Threading.Tasks;
    using Caliburn.Micro;
    using OxyPlot;
    using System;
    using Windows.Storage;

    public class PlotViewModel : ViewModelBase
    {
        public string ResultFileName { get; set; }

        public DateTime CreationDate
        {
            get { return creationDate; }
            set
            {
                if (value.Equals(creationDate)) return;
                creationDate = value;
                NotifyOfPropertyChange(() => CreationDate);
            }
        }

        public DateTime LastModificationDate
        {
            get { return lastModificationDate; }
            set
            {
                if (value.Equals(lastModificationDate)) return;
                lastModificationDate = value;
                NotifyOfPropertyChange(() => LastModificationDate);
            }
        }

        public string Description
        {
            get { return description; }
            set
            {
                if (value == description) return;
                description = value;
                NotifyOfPropertyChange(() => Description);
            }
        }

        public PlotModel PlotModel
        {
            get { return plotModel; }
            private set
            {
                if (Equals(value, plotModel)) return;
                plotModel = value;
                NotifyOfPropertyChange(() => PlotModel);
            }
        }
        private PlotModel plotModel;
        private DateTime creationDate;
        private DateTime lastModificationDate;

        private readonly IStorageFolder storageFolder;
        private string description;

        private readonly IAudiogramPlot audiogramPlot;

        public PlotViewModel(INavigationService navigationService,
            IAudiogramPlot audiogramPlot,
            IStorageFolder storageFolder) : base(navigationService)
        {
            this.storageFolder = storageFolder;
            this.audiogramPlot = audiogramPlot;
        }

        protected override async void OnActivate()
        {
            var resultFile = await storageFolder.GetFileAsync(ResultFileName);
            CreationDate = resultFile.DateCreated.DateTime;
            LastModificationDate = (await resultFile.GetBasicPropertiesAsync()).DateModified.DateTime;
            await audiogramPlot.CreateFromAsync(ResultFileName);
            PlotModel = audiogramPlot.PlotModel;
            Description = audiogramPlot.TestResult.Description;
        }

        public async Task UpdateAndSaveAsync()
        {
            audiogramPlot.TestResult.Description = Description;
            await audiogramPlot.SaveAllAndUpdateAsync();
            PlotModel = audiogramPlot.PlotModel;
        }


        public async Task DeleteAll()
        {
            var file = await storageFolder.GetFileAsync(audiogramPlot.LastUsedPath);
            await file.DeleteAsync();

            try
            {
                var svgFile = await storageFolder.GetFileAsync(AudiogramPathUtil.GetSvgFilePath(audiogramPlot.LastUsedPath));
                await svgFile.DeleteAsync();
            }
            catch (FileNotFoundException)
            {
            }
        }
    }
}
