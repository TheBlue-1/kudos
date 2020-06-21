#region
using System.Linq;
using System.Reflection;
using Kudos.Attributes;
#endregion

namespace Kudos.Models {
	public class CommandInfo {
		private const string Space = " ";
		public CommandParameterInfo[] AllParameter { get; }
		public Command Command { get; }
		public MethodInfo MethodInfo { get; }
		public CommandModuleInfo Module { get; }
		public CommandParameterInfo[] Parameter { get; }

		public CommandInfo(MethodInfo info, CommandModuleInfo module) {
			Module = module;
			MethodInfo = info;
			Command = info.GetCustomAttribute<Command>();
			AllParameter = MethodInfo.GetParameters().Select(parameterInfo => new CommandParameterInfo(parameterInfo)).ToArray();
			Parameter = AllParameter.Where(parameter => parameter.CommandParameter.Index >= 0).OrderBy(parameter => parameter.CommandParameter.Index).ToArray();
		}

		public override string ToString() {
			string info = Parameter.Aggregate(Command.Name, (current, parameter) => current + (Space + parameter));
			info += "\n";
			return info;
		}
	}
}
