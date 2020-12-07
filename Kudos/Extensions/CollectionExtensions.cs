#region
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
#endregion

namespace Kudos.Extensions {
	public static class CollectionExtensions {
		public static async Task<IEnumerable<T>> AwaitAll<T>(this IAsyncEnumerable<IReadOnlyCollection<T>> asyncCollection) {
			List<T> list = new List<T>();
			await asyncCollection.ForEachAsync(collection => { list.AddRange(collection); });
			return list;
		}

		public static ICollection<object> ToCollection(this ICollection collection) {
			ICollection<object> objectCollection = new Collection<object>();

			foreach (object o in collection) {
				objectCollection.Add(o);
			}
			return objectCollection;
		}
	}
}
