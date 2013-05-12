namespace PureToneAudiometer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using Coding4Fun.Toolkit.Controls;
    using Microsoft.Live;
    using Windows.Storage;

    public struct SkyDriveFile
    {
        private readonly string sourceFilePath;
        public string SourceFilePath { get { return sourceFilePath; } }

        private readonly string destinationFilePath;
        public string DestinationFilePath { get { return destinationFilePath; } }

        private readonly string plotSourceFilePath;
        public string PlotSourceFilePath { get { return plotSourceFilePath; } }

        private readonly string plotDestinationFilePath;
        public string PlotDestinationFilePath { get { return plotDestinationFilePath; } }

        public SkyDriveFile(string path, string description)
        {
            sourceFilePath = path;
            destinationFilePath = path.Insert(path.LastIndexOf('.'), "_" + description);
            plotSourceFilePath = AudiogramPathUtil.GetSvgFilePath(sourceFilePath);
            plotDestinationFilePath = plotSourceFilePath.Insert(plotSourceFilePath.LastIndexOf('.'), "_" + description);
        }

        public SkyDriveFile(string path)
        {
            destinationFilePath = sourceFilePath = path;
            plotDestinationFilePath = plotSourceFilePath = AudiogramPathUtil.GetSvgFilePath(path);
        }
    }

    public class SkyDriveUploadSummary
    {
        public int DataFilesTransferred { get; private set; }
        public int ChartFilesTransferred { get; private set; }
        public double DataKilobytesTransferred { get; private set; }
        public double ChartKilobytesTransferred { get; private set; }

        public SkyDriveUploadSummary(IList<ulong> dataFilesSize, IList<ulong> chartFilesSize)
        {
            DataFilesTransferred = dataFilesSize.Count;
            foreach (var size in dataFilesSize)
            {
                DataKilobytesTransferred += size;
            }
            DataKilobytesTransferred /= 1024;

            ChartFilesTransferred = chartFilesSize.Count;
            foreach (var size in chartFilesSize)
            {
                ChartKilobytesTransferred += size;
            }
            ChartKilobytesTransferred /= 1024;
        }
    }

    public class UploadEventArgs : EventArgs
    {
        public bool IsUploading { get; private set; }
        public UploadEventArgs(bool isUploading)
        {
            IsUploading = isUploading;
        }
    }

    public class MessageEventArgs : EventArgs
    {
        public string Message { get; private set; }
        public MessageEventArgs(string newMessage)
        {
            Message = newMessage;
        }
    }

    public class UploadFinishedEventArgs : EventArgs
    {
    }

    public interface ISkyDriveUpload
    {
        event EventHandler<UploadEventArgs> UploadChanged;
        event EventHandler<MessageEventArgs> MessageChanged;
        event EventHandler<UploadFinishedEventArgs>  FileUploadFinished;
        
        bool IsUploading { get; }
        bool IsInitialized { get; }
        string Message { get; }

        SkyDriveUploadSummary UploadSummary { get; }

        Task UploadAsync(IList<SkyDriveFile> files, IProgress<LiveOperationProgress> progress);

        Task InitializeAsync();
    }

    public class SkyDriveUpload : ISkyDriveUpload
    {
        private bool isUploading;
        private string message;

        public event EventHandler<UploadEventArgs> UploadChanged;
        public event EventHandler<MessageEventArgs> MessageChanged;
        public event EventHandler<UploadFinishedEventArgs> FileUploadFinished;

        private LiveConnectClient client;

        private readonly IStorageFolder folder;

        private bool shouldUploadPlots;

        private readonly ISettings applicationSettings;

        public SkyDriveUpload(IStorageFolder appStorageFolder, ISettings settings)
        {
            folder = appStorageFolder;
            applicationSettings = settings;
        }

        public bool IsUploading
        {
            get { return isUploading; }
            private set 
            {
                isUploading = value;
                OnUploadChanged();
            }
        }

        public bool IsInitialized { get; private set; }

        public string Message
        {
            get { return message; }
            private set 
            {
                message = value;
                OnMessageChanged();
            }
        }

        public SkyDriveUploadSummary UploadSummary { get; private set; }

        public async Task UploadAsync(IList<SkyDriveFile> files, IProgress<LiveOperationProgress> progress)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("Live client was not authenticated or initialized. Initialize the client first by calling InitializeAsync().");

            IsUploading = true;
            var dataSize = new List<ulong>(files.Count);
            var chartSize = new List<ulong>(files.Count);

            for (var i = 0; i < files.Count; ++i)
            {
                var file = files[i];
                var coreMessage = string.Format("Uploading packages: {0}/{1}", i + 1, files.Count);
                Message = coreMessage + " (XML data)";
                try
                {
                    var currentFile = await folder.GetFileAsync(file.SourceFilePath);
                    var basicProperties = await currentFile.GetBasicPropertiesAsync();
                    using (var stream = await currentFile.OpenStreamForReadAsync())
                    {
                        await client.UploadAsync("me/skydrive", file.DestinationFilePath, stream, OverwriteOption.Overwrite,
                                           CancellationToken.None, progress);
                    }
                    dataSize.Add(basicProperties.Size);
                    OnFileUploadFinished();
                }
                catch (LiveConnectException ex)
                {
                    var toast = new ToastPrompt
                    {
                        Title = "Connection error",
                        TextOrientation = Orientation.Horizontal,
                        Message = "File " + file.SourceFilePath + " was not uploaded"
                    };
                    toast.Completed += (sender, args) =>
                    {
                        if (args.PopUpResult == PopUpResult.Ok)
                        {
                            MessageBox.Show(ex.Message, "Connection error", MessageBoxButton.OK);
                        }
                    };
                    toast.Show();
                }

                if (!shouldUploadPlots) 
                    continue;

                Message = coreMessage + " (SVG plot)";
                try
                {
                    var plotFile = await folder.GetFileAsync(file.PlotSourceFilePath);
                    var basicProperties = await plotFile.GetBasicPropertiesAsync();
                    using (var stream = await plotFile.OpenStreamForReadAsync())
                    {
                        await
                            client.UploadAsync("me/skydrive", file.PlotDestinationFilePath, stream,
                                               OverwriteOption.Overwrite, CancellationToken.None, progress);
                    }
                    chartSize.Add(basicProperties.Size);
                    OnFileUploadFinished();
                }
                catch (LiveConnectException ex)
                {
                    var toast = new ToastPrompt
                                    {
                                        Title = "Connection error",
                                        TextOrientation = Orientation.Horizontal,
                                        Message = "Plot file " + file.PlotSourceFilePath + " was not uploaded"
                                    };
                    toast.Completed += (sender, args) =>
                                           {
                                               if (args.PopUpResult == PopUpResult.Ok)
                                               {
                                                   MessageBox.Show(ex.Message, "Connection error", MessageBoxButton.OK);
                                               }
                                           };
                    toast.Show();
                }
            }

            UploadSummary = new SkyDriveUploadSummary(dataSize, chartSize);

            IsUploading = false;
        }

        public async Task InitializeAsync()
        {
            shouldUploadPlots = applicationSettings.Get<bool>("ShouldAutomaticallyUploadPlots").GetOrElse(false);

            Message = "Authenticating";
            var auth = new LiveAuthClient("000000004C0EC1AB");

            var result = await auth.InitializeAsync(new[] { "wl.basic", "wl.offline_access", "wl.signin", "wl.skydrive_update" });
            if (result.Status != LiveConnectSessionStatus.Connected)
            {
                Message = "Logging in";
                result =
                    await
                    auth.LoginAsync(new[] { "wl.basic", "wl.offline_access", "wl.signin", "wl.skydrive_update" });
            }

            if (result.Status == LiveConnectSessionStatus.Connected)
            {
                client = new LiveConnectClient(result.Session);
                IsInitialized = true;
            }
        }

        private void OnMessageChanged()
        {
            var handler = MessageChanged;
            if (handler != null)
            {
                handler(this, new MessageEventArgs(Message));
            }
        }
        
        private void OnUploadChanged()
        {
            var handler = UploadChanged;
            if (handler != null)
            {
                handler(this, new UploadEventArgs(IsUploading));
            }
        }

        private void OnFileUploadFinished()
        {
            var handler = FileUploadFinished;
            if (handler != null)
            {
                handler(this, new UploadFinishedEventArgs());
            }
        }
    }
}
