#region

using Discord;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

#endregion

namespace Kudos.Utils {

    public class JsonIEmoteConverter : JsonConverter {
        private static readonly Type[] Types = { typeof(IEmote) };

        public override bool CanConvert(Type objectType) {
            return Types.Any(type => type.IsAssignableFrom(objectType));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            IEmote emote;
            JObject json = JObject.Load(reader);
            if (json.ContainsKey("Id")) {
                emote = Emote.Parse($"<:{json.Value<string>("Name")}:{json.Value<ulong>("Id")}>");
            } else {
                emote = new Emoji(json.Value<string>("Name"));
            }
            return emote;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            switch (value) {
                case Emote emote:
                serializer.Serialize(writer, new { emote.Name, emote.Id });
                break;

                case Emoji emoji:
                serializer.Serialize(writer, new { emoji.Name });
                break;

                default: throw new NotImplementedException();
            }
        }
    }
}
