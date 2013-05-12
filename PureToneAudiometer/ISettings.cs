namespace PureToneAudiometer
{
    using System.Collections.Generic;
    using Option;

    public interface ISettings
    {
        Option<T> Get<T>(string name);
        void Set<T>(string name, T value);
        void Clear();
        void MergeOverwrite(IDictionary<string, object> otherDictionary);
    }

    internal class Settings : ISettings
    {
        private readonly IDictionary<string, object> internalDictionary;
        public Settings(IDictionary<string, object> settingsDictionary)
        {
            internalDictionary = settingsDictionary;
        }

        public Option<T> Get<T>(string name)
        {
            object value;
            return internalDictionary.TryGetValue(name, out value) ? Option.From((T)value) : Option<T>.None;
        }

        public void Set<T>(string name, T value)
        {
            internalDictionary[name] = value;
        }

        public void Clear()
        {
            internalDictionary.Clear();
        }

        public void MergeOverwrite(IDictionary<string, object> otherDictionary)
        {
            foreach (var o in otherDictionary)
            {
                internalDictionary[o.Key] = o.Value;
            }
        }
    }
}
