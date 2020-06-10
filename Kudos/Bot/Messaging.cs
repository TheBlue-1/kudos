#region
using Discord.WebSocket;
#endregion

namespace Kudos.Bot {
	public sealed class Messaging {
		private const string HelloText = "hello ";
		private const string HelpText = @"```
hello                       answers hello
honor [count?] [user]       honors someone
dishonor [count?] [user]    dishonors someone
delete [count?]             deletes 1 or count messages
help                        sends this
? ...   optional
```";
		public static Messaging Instance { get; } = new Messaging();

		static Messaging() { }

		private Messaging() { }

		public void Hello(ISocketMessageChannel channel, string username) {
			channel.SendMessageAsync(HelloText + username);
		}

		public void Help(ISocketMessageChannel channel) {
			channel.SendMessageAsync(HelpText);
		}
	}
}
