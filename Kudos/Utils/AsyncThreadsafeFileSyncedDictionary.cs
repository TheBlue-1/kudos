#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable AssignmentIsFullyDiscarded
#endregion

namespace Kudos.Utils {
	public class AsyncThreadsafeFileSyncedDictionary<TKey, TValue> : IDictionary<TKey, TValue> {
		private Dictionary<TKey, TValue> _dictionaryImplementation;

		public int Count => RunLocked(() => DictionaryImplementation.Count);
		private Dictionary<TKey, TValue> DictionaryImplementation {
			get {
				if (_dictionaryImplementation == null) {
					ReadDictionary().Wait();
				}
				return _dictionaryImplementation;
			}
		}

		private string FileName { get; }
		public bool IsReadOnly => RunLocked(() => (DictionaryImplementation as ICollection<KeyValuePair<TKey, TValue>>).IsReadOnly);
		public ICollection<TKey> Keys => throw new NotSupportedException();
		public ICollection<TValue> Values => throw new NotSupportedException();

		public TValue this[TKey key] {
			get => RunLocked(() => DictionaryImplementation[key]);
			set {
				RunLocked(() => {
					DictionaryImplementation[key] = value;

					_ = SaveDictionary();
				});
			}
		}

		public AsyncThreadsafeFileSyncedDictionary(string fileName) => FileName = fileName;

		private async Task ReadDictionary() {
			_dictionaryImplementation = await FileService.Instance.ReadJsonFromFile<Dictionary<TKey, TValue>>(FileName);
		}

		private T RunLocked<T>(Func<T> func) {
			lock (DictionaryImplementation) {
				return func.Invoke();
			}
		}

		private void RunLocked(Action func) {
			lock (DictionaryImplementation) {
				func.Invoke();
			}
		}

		private async Task SaveDictionary() {
			await FileService.Instance.SaveJsonToFile(FileName, DictionaryImplementation);
		}

		public void Add(KeyValuePair<TKey, TValue> item) {
			RunLocked(() => {
				(DictionaryImplementation as ICollection<KeyValuePair<TKey, TValue>>).Add(item);

				_ = SaveDictionary();
			});
		}

		public void Clear() {
			RunLocked(() => {
				DictionaryImplementation.Clear();
				_ = SaveDictionary();
			});
		}

		public bool Contains(KeyValuePair<TKey, TValue> item) => RunLocked(() => DictionaryImplementation.Contains(item));

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
			RunLocked(() => (DictionaryImplementation as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex));
		}

		public bool Remove(KeyValuePair<TKey, TValue> item) {
			return RunLocked(() => {
				bool success = (DictionaryImplementation as ICollection<KeyValuePair<TKey, TValue>>).Remove(item);
				_ = SaveDictionary();
				return success;
			});
		}

		public void Add(TKey key, TValue value) {
			RunLocked(() => {
				DictionaryImplementation.Add(key, value);

				_ = SaveDictionary();
			});
		}

		public bool ContainsKey(TKey key) => RunLocked(() => DictionaryImplementation.ContainsKey(key));

		public bool Remove(TKey key) {
			return RunLocked(() => {
				bool success = DictionaryImplementation.Remove(key);
				_ = SaveDictionary();
				return success;
			});
		}

		public bool TryGetValue(TKey key, out TValue value) {
			lock (DictionaryImplementation) {
				return DictionaryImplementation.TryGetValue(key, out value);
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => throw new NotSupportedException();
	}
}
