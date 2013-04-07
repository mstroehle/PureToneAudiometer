namespace PureToneAudiometer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using Windows.Storage;

    public interface IXmlItemsFileManager<T> where T : class
    {
        string FileName { get; set; }
        Task<IEnumerable<T>> GetAsync();
        Task Save(IEnumerable<T> items);
        Task AddAsync(T item);
        Task UpdateAsync(T item, Func<T, bool> predicate);
        Task UpdateOrAddAsync(T item, Func<T, bool> predicate);
        Task RemoveAsync(T item);
        Task RemoveAsync(Func<T, bool> predicate);
    }

    public class XmlItemsFileManager<T> : IXmlItemsFileManager<T> where T : class
    {
        protected readonly IStorageFolder StorageFolder;

        public string FileName { get; set; }

        protected readonly DataContractSerializer Serializer;

        public XmlItemsFileManager(IStorageFolder folder, string fileName )
        {
            StorageFolder = folder;
            FileName = fileName;
            Serializer = new DataContractSerializer(typeof(List<T>));
        }

        public async Task<IEnumerable<T>> GetAsync()
        {
            try
            {
                var file = await StorageFolder.GetFileAsync(FileName);
                using (var stream = await file.OpenStreamForReadAsync())
                {
                    var items = Serializer.ReadObject(stream) as List<T>;
                    return items ?? new List<T>();
                }
            }
            catch (FileNotFoundException)
            {
                return new List<T>();
            }
        }

        public virtual async Task Save(IEnumerable<T> items)
        {
            var file = await StorageFolder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                Serializer.WriteObject(stream, items.ToList());
            }
        }

        public async Task AddAsync(T item)
        {
            var items = (await GetAsync()).ToList();

            items.Add(item);

            await Save(items);
        }

        public async Task UpdateAsync(T item, Func<T, bool> predicate)
        {
            var items = (await GetAsync()).ToList();

            var currentItem = items.Single(predicate);
            
            items.Remove(currentItem);
            items.Add(item);

            await Save(items);
        } 

        public async Task UpdateOrAddAsync(T item, Func<T, bool> predicate)
        {
            var items = (await GetAsync()).ToList();
            var currentItem = items.SingleOrDefault(predicate);
            if (currentItem == null)
            {
                await AddAsync(item);
            }
            else
            {
                await UpdateAsync(item, predicate);
            }
        }

        public async Task RemoveAsync(T item)
        {
            var items = (await GetAsync()).ToList();
            items.Remove(item);
            await Save(items);
        }

        public async Task RemoveAsync(Func<T, bool> predicate)
        {
            var items = (await GetAsync()).ToList();
            var item = items.SingleOrDefault(predicate);

            if (item == null)
                return;

            items.Remove(item);

            await Save(items);
        }
    }
}
