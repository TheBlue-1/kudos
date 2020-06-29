#region
using System.IO;
using System.Threading.Tasks;
using Discord.Audio;
using Discord.WebSocket;
using Kudos.Attributes;
using Kudos.Exceptions;
using Kudos.Extensions;
#endregion

namespace Kudos.Bot.Modules {
	[CommandModule("Stalker")]
	public sealed class Stalker {
		public static Stalker Instance { get; } = new Stalker();

		static Stalker() { }

		private Stalker() { }

		[Command("stalk")]
		public async Task Stalk([CommandParameter]SocketUser author,[CommandParameter(0)] SocketUser victim) {
			ISocketAudioChannel channel = victim.AudioChannel();
			if (channel == null) {
				throw new KudosArgumentException("user not in voice channel");
			}
			IAudioClient client = await channel.ConnectAsync();
			client.StreamCreated += async (id, stream) => {
				byte[] buffer = new byte[1024];
				FileStream file = File.OpenWrite("D:\\Downloads\\test.audio");
				while (true) {
					await stream.ReadAsync(buffer, 0, 1024);
					await file.WriteAsync(buffer, 0, 1024);
				}
			};
			client.CreateDirectOpusStream();
		}
	}
}
