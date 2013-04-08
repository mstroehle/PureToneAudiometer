namespace PureToneAudiometer
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAsyncXmlFileManager
    {
        string FileName { get; set; }
        Task<IEnumerable<T>> GetCollection<T>()  where T : class; 
        Task<T> Get<T>() where T : class;
        Task Save<T>(IEnumerable<T> items)  where T : class;
        Task Save<T>(T item) where T : class;
        Task AddToCollection<T>(T item)  where T : class;
        Task UpdateCollection<T>(T item, Func<T, bool> predicate) where T : class;
        Task UpdateOrAddToCollection<T>(T item, Func<T, bool> predicate) where T : class;
        Task RemoveFromCollection<T>(T item) where T : class;
        Task RemoveFromCollection<T>(Func<T, bool> predicate) where T : class;
    }
}
