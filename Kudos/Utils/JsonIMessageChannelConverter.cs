#region
using System;
using System.Linq;
using Discord;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endregion

namespace Kudos.Utils {
	public class JsonIMessageChannelConverter : JsonConverter {
		private static readonly Type[] Types = { typeof (IMessageChannel) };

		public override bool CanConvert(Type objectType) {
			return Types.Any(type => type.IsAssignableFrom(objectType));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			if (reader.TokenType != JsonToken.StartObject) {
				return null;
			}
			JObject json = JObject.Load(reader);

			return Program.Client.GetMessageChannelById(json.Value<ulong>("Id"));
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			if (value is IMessageChannel channel) {
				serializer.Serialize(writer, new { channel.Id });
			}
		}
	}
}
