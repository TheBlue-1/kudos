#region
using System;
using System.Linq;
using Discord;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endregion

namespace Kudos.Utils {
	public class JsonIEmoteConverter : JsonConverter {
		private static readonly Type[] Types = { typeof (IEmote) };

		public override bool CanConvert(Type objectType) => Types.Contains(objectType);

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			IEmote emote;
			JObject json = JObject.Load(reader);
			if (json.ContainsKey("Id")) {
				emote = Emote.Parse($"<:{json.Value<string>("Name")}:{json.Value<ulong>("id")}>");
			} else {
				emote = new Emoji(json.Value<string>("Name"));
			}
			return emote;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			switch (value) {
				case Emote emote :
					serializer.Serialize(writer, emote);
					break;
				case Emoji emoji :
					serializer.Serialize(writer, emoji);
					break;
				default : throw new NotImplementedException();
			}
		}
	}
}
