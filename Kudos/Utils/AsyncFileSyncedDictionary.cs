#region
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#endregion

namespace Kudos.Utils {
	public class AsyncFileSyncedDictionary<TKey, TValue> : IDictionary<TKey, TValue> {
		private Dictionary<TKey, TValue> _dictionaryImplementation;

		public int Count => DictionaryImplementation.Count;
		private Dictionary<TKey, TValue> DictionaryImplementation {
			get {
				if (_dictionaryImplementation == null) {
					ReadDictionary().Wait();
				}
				if (!HasExclusiveFileOwnership) {
					_ = ReadDictionary();
				}
				return _dictionaryImplementation;
			}
		}
		private SerializationFormat Format { get; }
		private string FileName { get; }
		private bool HasExclusiveFileOwnership { get; }
		public bool IsReadOnly => (DictionaryImplementation as ICollection<KeyValuePair<TKey, TValue>>).IsReadOnly;
		public ICollection<TKey> Keys => DictionaryImplementation.Keys;
		public ICollection<TValue> Values => DictionaryImplementation.Values;

		public TValue this[TKey key] {
			get => DictionaryImplementation[key];
			set {
				DictionaryImplementation[key] = value;
				_ = SaveDictionary();
			}
		}

		public enum SerializationFormat {
			Binary,
			Json
		}

		public AsyncFileSyncedDictionary(string fileName, bool hasExclusiveFileOwnership = true,SerializationFormat format = SerializationFormat.Binary) {
			Format = format;
			FileName = fileName;
			HasExclusiveFileOwnership = hasExclusiveFileOwnership;
		}

		private async Task ReadDictionary() {
			switch (Format)
			{
				case SerializationFormat.Binary: _dictionaryImplementation = await FileService.Instance.ReadFromFile<Dictionary<TKey, TValue>>(FileName);break;
				case SerializationFormat.Json: _dictionaryImplementation = await FileService.Instance.ReadJsonFromFile<Dictionary<TKey, TValue>>(FileName); break;
			}
			
		}

		private async Task SaveDictionary() {
			switch (Format)
			{
				case SerializationFormat.Binary: await FileService.Instance.SaveToFile(FileName, DictionaryImplementation); break;
				case SerializationFormat.Json: await FileService.Instance.SaveJsonToFile(FileName, DictionaryImplementation); break;
			}
			
		}

		public void Add(KeyValuePair<TKey, TValue> item) {
			(DictionaryImplementation as ICollection<KeyValuePair<TKey, TValue>>).Add(item);
			_ = SaveDictionary();
		}

		public void Clear() {
			DictionaryImplementation.Clear();
			_ = SaveDictionary();
		}

		public bool Contains(KeyValuePair<TKey, TValue> item) => DictionaryImplementation.Contains(item);

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
			(DictionaryImplementation as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex);
		}

		public bool Remove(KeyValuePair<TKey, TValue> item) {
			bool success = (DictionaryImplementation as ICollection<KeyValuePair<TKey, TValue>>).Remove(item);
			_ = SaveDictionary();
			return success;
		}

		public void Add(TKey key, TValue value) {
			DictionaryImplementation.Add(key, value);
			_ = SaveDictionary();
		}

		public bool ContainsKey(TKey key) => DictionaryImplementation.ContainsKey(key);

		public bool Remove(TKey key) {
			bool success = DictionaryImplementation.Remove(key);
			_ = SaveDictionary();
			return success;
		}

		public bool TryGetValue(TKey key, out TValue value) => DictionaryImplementation.TryGetValue(key, out value);

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)DictionaryImplementation).GetEnumerator();
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => DictionaryImplementation.GetEnumerator();
	}
}
