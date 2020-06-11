#region
using Discord.WebSocket;
#endregion

namespace Kudos.Bot {
	public sealed class Messaging {
		private const string HelloText = "hello ";
		private const string HelpText = @"```
hello                           answers hello
honor [count?] [user]           honors someone
dishonor [count?] [user]        dishonors someone
balance [user]                  shows the honor balance of user
delete [count?]                 deletes 1 or count messages
question [user] [question]      sends anonymous question to person
answer [questionId] [answer]    answers anonymous question
help                            sends this
```
```
[user]      mention with @ (if not possible use full username like '@Kudos#9294')
[x?]	    optional
```";

		public static Messaging Instance { get; } = new Messaging();

		static Messaging() { }

		private Messaging() { }

		public void Hello(ISocketMessageChannel channel, SocketUser user) {
			channel.SendMessageAsync(HelloText + user.Mention);
		}

		public void Help(ISocketMessageChannel channel) {
			channel.SendMessageAsync(HelpText);
		}

		public void Message(ISocketMessageChannel channel, string text) {
			channel.SendMessageAsync(text);
		}
	}
}
