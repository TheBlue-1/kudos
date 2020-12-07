#region
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#endregion

namespace Kudos.Extensions {
	public static class CollectionExtensions {
		public static ICollection<object> ToCollection(this ICollection collection) {
			ICollection<object> objectCollection = new Collection<object>();

			foreach (object o in collection) {
				objectCollection.Add(o);
			}
			return objectCollection;
		}
	}
}
