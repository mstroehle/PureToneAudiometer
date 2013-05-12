namespace PureToneAudiometer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
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

        private readonly IDictionary<string, object> applicationSettings;

        public SkyDriveUpload(IStorageFolder appStorageFolder, IDictionary<string, object> settings)
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

        public async Task UploadAsync(IList<SkyDriveFile> files, IProgress<LiveOperationProgress> progress)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("Live client was not authenticated or initialized. Initialize the client first by calling InitializeAsync().");

            IsUploading = true;
            for (var i = 0; i < files.Count; ++i)
            {
                var file = files[i];
                var coreMessage = string.Format("Uploading file packs: {0}/{1}", i + 1, files.Count);
                Message = coreMessage + " (Data)";
                try
                {
                    var currentFile = await folder.GetFileAsync(file.SourceFilePath);
                    using (var stream = await currentFile.OpenStreamForReadAsync())
                    {
                        await client.UploadAsync("me/skydrive", file.DestinationFilePath, stream, OverwriteOption.Overwrite,
                                           CancellationToken.None, progress);
                    }
                    OnFileUploadFinished();
                    
                }
                catch (LiveConnectException)
                {
                    var toast = new ToastPrompt
                    {
                        Title = "Connection error",
                        TextOrientation = Orientation.Horizontal,
                        Message = "File " + file.SourceFilePath + " was not uploaded"
                    };
                    toast.Show();
                }

                if (!shouldUploadPlots) 
                    continue;

                Message = coreMessage + " (Plot)";
                try
                {
                    var plotFile = await folder.GetFileAsync(file.PlotSourceFilePath);
                    using (var stream = await plotFile.OpenStreamForReadAsync())
                    {
                        await
                            client.UploadAsync("me/skydrive", file.PlotDestinationFilePath, stream,
                                               OverwriteOption.Overwrite, CancellationToken.None, progress);
                    }
                    OnFileUploadFinished();
                }
                catch (LiveConnectException)
                {
                    var toast = new ToastPrompt
                                    {
                                        Title = "Connection error",
                                        TextOrientation = Orientation.Horizontal,
                                        Message = "Plot file " + file.PlotSourceFilePath + " was not uploaded"
                                    };
                    toast.Show();
                }
            }
            IsUploading = false;
        }

        public async Task InitializeAsync()
        {
            object val;
            if (applicationSettings.TryGetValue("ShouldAutomaticallyUploadPlots", out val))
            {
                shouldUploadPlots = (bool) val;
            }
            else
            {
                shouldUploadPlots = false;
            }

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
