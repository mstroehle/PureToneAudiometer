namespace PureToneAudiometer
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public interface IReadOnlyTraversableList<out T> : IReadOnlyList<T>
    {
        bool CanGetNext { get; }
        T Next();

        bool CanGetPrevious { get; }
        T Previous();

        void ResetTraversal();
    }

    public class ReadOnlyTraversableList<T> : IReadOnlyTraversableList<T>
    {
        private readonly IReadOnlyList<T> list;
        private int currentTraversalIndex;

        public ReadOnlyTraversableList() : this(new List<T>())
        {
        }

        public ReadOnlyTraversableList(IEnumerable<T> source)
        {
            list = source.ToList();
            currentTraversalIndex = -1;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count { get { return list.Count; } }

        public T this[int index]
        {
            get { return list[index]; }
        }

        public bool CanGetNext { get { return currentTraversalIndex < list.Count - 1; } }

        public T Next()
        {
            return this[++currentTraversalIndex];
        }

        public bool CanGetPrevious { get { return currentTraversalIndex > 0; } }
        public T Previous()
        {
            return this[--currentTraversalIndex];
        }

        public void ResetTraversal()
        {
            currentTraversalIndex = -1;
        }
    }
}
