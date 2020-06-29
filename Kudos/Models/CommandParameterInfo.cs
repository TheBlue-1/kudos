#region
using System;
using System.Collections.Generic;
using System.Reflection;
using Discord.WebSocket;
using Kudos.Attributes;
using Kudos.Utils;
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
			string info = $"[{ParameterType.ParameterTypes[ParameterInfo.ParameterType].Character}|{ParameterInfo.Name}{(CommandParameter.Optional ? "?" : "")}]";
			return info;
		}

		
	}
}
