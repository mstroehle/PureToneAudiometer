namespace PureToneAudiometer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using Windows.Storage;

    public class AsyncXmlFileManager : IAsyncXmlFileManager
    {
        protected readonly IStorageFolder StorageFolder;

        public string FileName { get; set; }

        protected DataContractSerializer Serializer;

        public AsyncXmlFileManager(IStorageFolder folder)
        {
            StorageFolder = folder;
        }

        public async Task<IEnumerable<T>> GetCollection<T>() where T : class
        {
            try
            {
                var file = await StorageFolder.GetFileAsync(FileName);
                using (var stream = await file.OpenStreamForReadAsync())
                {
                    Serializer = new DataContractSerializer(typeof(List<T>));
                    var items = Serializer.ReadObject(stream) as List<T>;
                    return items ?? new List<T>();
                }
            }
            catch (FileNotFoundException)
            {
                return new List<T>();
            }
        }

        public async Task<T> Get<T>() where T : class
        {
            try
            {
                var file = await StorageFolder.GetFileAsync(FileName);
                using (var stream = await file.OpenStreamForReadAsync())
                {
                    Serializer = new DataContractSerializer(typeof(T));
                    return Serializer.ReadObject(stream) as T;
                }
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        public virtual async Task Save<T>(IEnumerable<T> items) where T : class
        {
            var file = await StorageFolder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                Serializer = new DataContractSerializer(typeof(List<T>));
                Serializer.WriteObject(stream, items.ToList());
            }
        }

        public async Task Save<T>(T item) where T : class
        {
            var file = await StorageFolder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                Serializer = new DataContractSerializer(typeof(T));
                Serializer.WriteObject(stream, item);
            }
        }

        public async Task AddToCollection<T>(T item) where T : class
        {
            var items = (await GetCollection<T>()).ToList();

            items.Add(item);

            await Save(items);
        }

        public async Task UpdateCollection<T>(T item, Func<T, bool> predicate) where T : class
        {
            var items = (await GetCollection<T>()).ToList();

            var currentItem = items.Single(predicate);
            
            items.Remove(currentItem);
            items.Add(item);

            await Save(items);
        } 

        public async Task UpdateOrAddToCollection<T>(T item, Func<T, bool> predicate) where T : class
        {
            var items = (await GetCollection<T>()).ToList();
            var currentItem = items.SingleOrDefault(predicate);
            if (currentItem == null)
            {
                await AddToCollection(item);
            }
            else
            {
                await UpdateCollection(item, predicate);
            }
        }

        public async Task RemoveFromCollection<T>(T item) where T : class
        {
            var items = (await GetCollection<T>()).ToList();
            items.Remove(item);
            await Save(items);
        }

        public async Task RemoveFromCollection<T>(Func<T, bool> predicate) where T : class
        {
            var items = (await GetCollection<T>()).ToList();
            var item = items.SingleOrDefault(predicate);

            if (item == null)
                return;

            items.Remove(item);

            await Save(items);
        }
    }
}