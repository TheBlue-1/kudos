#region
using Kudos.Exceptions;
#endregion

namespace Kudos.Models {
	public class Timezone {
		private double Value { get; }

		private Timezone(double value) {
			if (value > 12 || value < -12) {
				throw new KudosArgumentException("timezone must be a number from -12 to 12");
			}
			Value = value;
		}

		public static implicit operator Timezone(double value) => new Timezone(value);
		public static implicit operator double(Timezone timezone) => timezone.Value;
	}
}
