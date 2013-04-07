namespace PureToneAudiometer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using ViewModels.Start;
    using Windows.Storage;

    public class RecentItemManager : IXmlItemsFileManager<RecentItemViewModel>
    {
        private readonly IStorageFolder storageFolder;

        public string FileName { get;  set; }

        private readonly DataContractSerializer serializer;


        public RecentItemManager(IStorageFolder folder, string fileName)
        {
            storageFolder = folder;
            FileName = fileName;
            serializer = new DataContractSerializer(typeof(List<RecentItemViewModel>));
        }

        public async Task<IEnumerable<RecentItemViewModel>> GetAsync()
        {
            try
            {
                var file = await storageFolder.GetFileAsync(FileName);
                using (var stream = await file.OpenStreamForReadAsync())
                {
                    var items = serializer.ReadObject(stream) as List<RecentItemViewModel>;
                    return items ?? new List<RecentItemViewModel>();
                }
            }
            catch (FileNotFoundException)
            {
                return new List<RecentItemViewModel>();
            }
        }

        public async Task AddAsync(RecentItemViewModel item)
        {
            var items = (await GetAsync()).ToList();

            items.Add(item);

            await Save(items.OrderByDescending(x => x.LastUsedDate));
        }

        public async Task UpdateAsync(RecentItemViewModel item, Func<RecentItemViewModel, bool> predicate)
        {
            var items = (await GetAsync()).ToList();

            var existingItem = items.Single(predicate);

            existingItem.LastUsedDate = item.LastUsedDate;

            await Save(items.OrderByDescending(x => x.LastUsedDate));
        }

        public async Task UpdateOrAddAsync(RecentItemViewModel item, Func<RecentItemViewModel, bool> predicate)
        {
            var items = (await GetAsync()).ToList();

            var existingItem = items.SingleOrDefault(predicate);
            if (existingItem == null)
            {
                await AddAsync(item);
            }
            else
            {
                await UpdateAsync(item, predicate);
            }
        }

        public async Task RemoveAsync(RecentItemViewModel item)
        {
            var items = (await GetAsync()).ToList();

            items.Remove(item);

            await Save(items);
        }

        public async Task RemoveAsync(Func<RecentItemViewModel, bool> predicate)
        {
            var items = (await GetAsync()).ToList();

            var existingItem = items.SingleOrDefault(predicate);

            if (existingItem == null)
                return;

            items.Remove(existingItem);

            await Save(items);
        }

        public async Task Save(IEnumerable<RecentItemViewModel> items)
        {
            var file = await storageFolder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);

            using (var stream = await file.OpenStreamForWriteAsync())
            {
                serializer.WriteObject(stream, items.ToList());
            }
        }
    }
}

