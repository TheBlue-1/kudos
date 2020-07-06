#region
using System.Reflection;
using Kudos.Attributes;
using Kudos.Bot;
#endregion

namespace Kudos.Models {
	public class CommandParameterInfo {
		public CommandParameter CommandParameter { get; }
		public ParameterInfo ParameterInfo { get; }

		public CommandParameterInfo(ParameterInfo info) {
			ParameterInfo = info;
			CommandParameter = info.GetCustomAttribute<CommandParameter>();
		}

		public override string ToString() {
			string info =
				$"[{ParameterType.ParameterTypes[ParameterInfo.ParameterType].Character}|{ParameterInfo.Name}{(CommandParameter.Optional ? "?" : "")}]";
			return info;
		}
	}
}
