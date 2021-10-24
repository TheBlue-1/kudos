#region

using System.Collections;
using System.Collections.Generic;

#endregion

namespace Kudos.Extensions {

    public static class DictionaryExtensions {

        public static IDictionary<object, object> ToDictionary(this IDictionary dictionary) {
            IDictionary<object, object> objectDictionary = new Dictionary<object, object>();

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (DictionaryEntry o in dictionary) {
                objectDictionary.Add(o.Key, o.Value);
            }
            return objectDictionary;
        }
    }
}