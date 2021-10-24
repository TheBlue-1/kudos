#region

using Kudos.Attributes;
using Kudos.Bot;
using System.Reflection;

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
                $"[{ParameterType.FromType(ParameterInfo.ParameterType).Character}|{ParameterInfo.Name}{(CommandParameter.Optional ? "?" : "")}{(CommandParameter.DefaultValue.Value.IsSpecified ? $"({CommandParameter.DefaultValue.Value})" : CommandParameter.DefaultValue.Special == ParameterType.SpecialDefaults.None ? "" : "(auto)")}]";
            return info;
        }
    }
}