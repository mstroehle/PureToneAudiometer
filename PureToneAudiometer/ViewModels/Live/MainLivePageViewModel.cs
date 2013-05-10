namespace PureToneAudiometer.ViewModels.Live
{
    using System;
    using System.Linq;
    using Caliburn.Micro;
    using Microsoft.Live;

    public class MainLivePageViewModel : ViewModelBase, IProgress<LiveOperationProgress>
    {
        public string CombinedPath { get; set; }

        private bool isBusy;
        public bool IsBusy
        {
            get { return isBusy; }
            private set
            {
                if (value.Equals(isBusy)) return;
                isBusy = value;
                NotifyOfPropertyChange(() => IsBusy);
            }
        }

        public double UploadPercentage
        {
            get { return uploadPercentage; }
            private set
            {
                if (value.Equals(uploadPercentage)) return;
                uploadPercentage = value;
                NotifyOfPropertyChange(() => UploadPercentage);
            }
        }

        public bool IsUploading
        {
            get { return isUploading; }
            private set
            {
                if (value.Equals(isUploading)) return;
                isUploading = value;
                NotifyOfPropertyChange(() => IsUploading);
            }
        }

        public string Message
        {
            get { return message; }
            private set
            {
                if (value == message) return;
                message = value;
                NotifyOfPropertyChange(() => Message);
            }
        }

        private string message;
        private double uploadPercentage;
        private bool isUploading;

        private readonly ISkyDriveUpload skyDriveUpload;

        public MainLivePageViewModel(INavigationService navigationService, ISkyDriveUpload skyDriveUpload) : base(navigationService)
        {
            this.skyDriveUpload = skyDriveUpload;
        }

        protected override async void OnActivate()
        {
            IsBusy = true;

            skyDriveUpload.MessageChanged += (sender, args) => Message = args.Message;
            skyDriveUpload.UploadChanged += (sender, args) => IsUploading = args.IsUploading;

            await skyDriveUpload.InitializeAsync();

            await
                skyDriveUpload.UploadAsync(
                    CombinedPath.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => new SkyDriveFile(x)).ToList(), this);

            IsBusy = false;
        }

        public void Report(LiveOperationProgress value)
        {
            UploadPercentage = value.ProgressPercentage;
        }
    }
}
