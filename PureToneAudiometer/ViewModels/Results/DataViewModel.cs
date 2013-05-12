namespace PureToneAudiometer.ViewModels.Results
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Audio;
    using Caliburn.Micro;
    using Core;


    public class Group<TKey, TValue> :  List<TValue>
    {
        public TKey Key { get; private set; }
        
        public Group(TKey key)
        {
            Key = key;
        }

        public Group(TKey key, IEnumerable<TValue> values)
        {
            Key = key;
            AddRange(values);
        }

    }

    public static class Group
    {
        public static IEnumerable<Group<TKey, TValue>> CreateGroups<TKey, TValue>(IEnumerable<TValue> items,
                                                                     Func<TValue, TKey> keySelector)
        {
            return items.GroupBy(keySelector).Select(x => new Group<TKey, TValue>(x.Key, x));
        }
    }

    public class DataViewModel : ViewModelBase
    {
        public class DataItem
        {
            public Channel Channel { get; set; }
            public int Frequency { get; set; }
            public int Volume { get; set; }
        }

        public string ResultFileName { get; set; }

        private readonly IAsyncXmlFileManager fileManager;
        private string description;
        private IReadOnlyList<Group<Channel, DataItem>> items;

        public string Description
        {
            get { return description; }
            private set
            {
                if (value == description) return;
                description = value;
                NotifyOfPropertyChange(() => Description);
            }
        }

        public IReadOnlyList<Group<Channel, DataItem>> Items
        {
            get { return items; }
            private set
            {
                if (Equals(value, items)) return;
                items = value;
                NotifyOfPropertyChange(() => Items);
            }
        }


        public DataViewModel(INavigationService navigationService, IAsyncXmlFileManager fileManager) : base(navigationService)
        {
            this.fileManager = fileManager;
        }

        protected async override void OnActivate()
        {
            fileManager.FileName = ResultFileName;
            var results = await fileManager.Get<TestResult>();
            Description = results.Description;
            var source = results.LeftChannel.Select(x => new DataItem
                                                        {
                                                            Channel = Channel.Left,
                                                            Frequency = x.Frequency,
                                                            Volume = x.Volume
                                                        })
                           .Concat(results.RightChannel.Select(x => new DataItem
                                                                        {
                                                                            Channel = Channel.Right,
                                                                            Frequency = x.Frequency,
                                                                            Volume = x.Volume
                                                                        }));
            Items = Group.CreateGroups(source, item => item.Channel).ToList();

        }
    }
}
