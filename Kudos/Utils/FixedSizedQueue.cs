#region

using System.Collections.Concurrent;

#endregion

namespace Kudos.Utils {

    public class FixedSizedQueue<T> : ConcurrentQueue<T> {
        private readonly object _syncObject = new();

        public int Size { get; }

        public FixedSizedQueue(int size) => Size = size;

        public new void Enqueue(T obj) {
            base.Enqueue(obj);
            lock (_syncObject) {
                while (Count > Size) {
                    TryDequeue(out T _);
                }
            }
        }
    }
}