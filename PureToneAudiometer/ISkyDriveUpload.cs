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
        private readonly string filePath;
        public string FilePath{get { return filePath; }}
        
        public SkyDriveFile(string path, string description)
        {
            filePath = path.Insert(path.LastIndexOf('.'), "_" + description);
        }

        public SkyDriveFile(string path)
        {
            filePath = path;
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

    public interface ISkyDriveUpload
    {
        event EventHandler<UploadEventArgs> UploadChanged;
        event EventHandler<MessageEventArgs> MessageChanged;
        
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
        
        private LiveConnectClient client;

        private readonly IStorageFolder folder;

        public SkyDriveUpload(IStorageFolder appStorageFolder)
        {
            folder = appStorageFolder;
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
            IsUploading = true;
            var i = 1;
            foreach (var file in files)
            {
                Message = string.Format("Uploading files: {0}/{1}", i,
                                        files.Count);
                try
                {
                    var currentFile = await folder.GetFileAsync(file.FilePath);
                    using (var stream = await currentFile.OpenStreamForReadAsync())
                    {
                        await client.UploadAsync("me/skydrive", file.FilePath, stream, OverwriteOption.Overwrite,
                                           CancellationToken.None, progress);
                    }
                }
                catch (LiveConnectException)
                {
                    var toast = new ToastPrompt
                    {
                        Title = "Error",
                        TextOrientation = Orientation.Horizontal,
                        Message = "File " + file.FilePath + " was not uploaded"
                    };
                    toast.Show();
                }
                i++;
            }
            IsUploading = false;
        }

        public async Task InitializeAsync()
        {
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
    }
}
