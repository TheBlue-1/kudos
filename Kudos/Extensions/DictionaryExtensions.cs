#region
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Kudos.Extensions {
	public static class DictionaryExtensions {
		public static Dictionary<object, object> ToDictionary(this IDictionary dictionary) {
			return dictionary.Cast<DictionaryEntry>().ToDictionary(o => o.Key, o => o.Value);
		}
	}
}
