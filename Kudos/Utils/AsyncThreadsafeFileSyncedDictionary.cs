﻿#region

using Kudos.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable AssignmentIsFullyDiscarded
#endregion

namespace Kudos.Utils {

    public class AsyncThreadsafeFileSyncedDictionary<TKey, TValue> : IDictionary<TKey, TValue> {
        private readonly object _fileLockObject = new();
        private Dictionary<TKey, TValue> _dictionaryImplementation;
        public int Count => RunLocked(() => DictionaryImplementation.Count);

        protected Dictionary<TKey, TValue> DictionaryImplementation {
            get {
                if (_dictionaryImplementation == null) {
                    ReadDictionary().Wait();
                }
                return _dictionaryImplementation;
            }
        }

        private string FileName { get; }

        // ReSharper disable once UnusedMember.Global
        public ImmutableDictionary<TKey, TValue> Immutable => RunLocked(() => DictionaryImplementation.ToImmutableDictionary());

        public bool IsReadOnly => RunLocked(() => (DictionaryImplementation as ICollection<KeyValuePair<TKey, TValue>>).IsReadOnly);
        public ICollection<TKey> Keys => throw new NotSupportedException();
        public ICollection<TValue> Values => throw new NotSupportedException();

        public virtual TValue this[TKey key] {
            get => RunLocked(() => DictionaryImplementation[key]);
            set {
                RunLocked(() => {
                    DictionaryImplementation[key] = value;

                    SaveDictionary().RunAsyncSave();
                });
            }
        }

        public AsyncThreadsafeFileSyncedDictionary(string fileName) => FileName = fileName;

        protected async Task ReadDictionary() {
            await Task.Run(() => {
                lock (_fileLockObject) {
                    _dictionaryImplementation = FileService.Instance.ReadJsonFromFile<Dictionary<TKey, TValue>>(FileName).WaitForResult();
                }
            });
        }

        protected T RunLocked<T>(Func<T> func) {
            lock (DictionaryImplementation) {
                return func.Invoke();
            }
        }

        protected void RunLocked(Action func) {
            lock (DictionaryImplementation) {
                func.Invoke();
            }
        }

        protected async Task SaveDictionary() {
            await Task.Run(() => {
                lock (_fileLockObject) {
                    FileService.Instance.SaveJsonToFile(FileName, DictionaryImplementation).Wait();
                }
            });
        }

        public virtual void Add(KeyValuePair<TKey, TValue> item) {
            RunLocked(() => {
                (DictionaryImplementation as ICollection<KeyValuePair<TKey, TValue>>).Add(item);

                SaveDictionary().RunAsyncSave();
            });
        }

        public virtual void Clear() {
            RunLocked(() => {
                DictionaryImplementation.Clear();
                SaveDictionary().RunAsyncSave();
            });
        }

        public virtual bool Contains(KeyValuePair<TKey, TValue> item) => RunLocked(() => DictionaryImplementation.Contains(item));

        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            RunLocked(() => (DictionaryImplementation as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex));
        }

        public virtual bool Remove(KeyValuePair<TKey, TValue> item) {
            return RunLocked(() => {
                bool success = (DictionaryImplementation as ICollection<KeyValuePair<TKey, TValue>>).Remove(item);
                SaveDictionary().RunAsyncSave();
                return success;
            });
        }

        public virtual void Add(TKey key, TValue value) {
            RunLocked(() => {
                DictionaryImplementation.Add(key, value);

                SaveDictionary().RunAsyncSave();
            });
        }

        public virtual bool ContainsKey(TKey key) => RunLocked(() => DictionaryImplementation.ContainsKey(key));

        public virtual bool Remove(TKey key) {
            return RunLocked(() => {
                bool success = DictionaryImplementation.Remove(key);
                SaveDictionary().RunAsyncSave();
                return success;
            });
        }

        public virtual bool TryGetValue(TKey key, out TValue value) {
            lock (DictionaryImplementation) {
                return DictionaryImplementation.TryGetValue(key, out value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => throw new NotSupportedException();
    }
}