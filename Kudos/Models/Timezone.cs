#region

using Kudos.Exceptions;
using Newtonsoft.Json;

#endregion

namespace Kudos.Models {

    public class Timezone {

        [JsonProperty]
        private double Value { get; }

        [JsonConstructor]
        private Timezone(double value) {
            if (value > 12 || value < -12) {
                throw new KudosArgumentException("timezone must be a number from -12 to 12");
            }
            Value = value;
        }

        public static implicit operator Timezone(double value) => new(value);

        public static implicit operator double(Timezone timezone) => timezone.Value;
    }
}