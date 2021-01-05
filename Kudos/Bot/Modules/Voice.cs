#region
using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.Audio;
using Discord.WebSocket;
using Kudos.Attributes;
using Kudos.Exceptions;
using Kudos.Extensions;
using NAudio.Wave;
#endregion

namespace Kudos.Bot.Modules {
	[CommandModule("Voice")]
	public sealed class Voice {
		public static Voice Instance { get; } = new Voice();

		static Voice() { }

		private Voice() { }

		[Command("test")]
		public async Task Test([CommandParameter] SocketUser author, [CommandParameter(0)] SocketUser victim) {
			ISocketAudioChannel channel = victim.AudioChannel();
			if (channel == null) {
				throw new KudosArgumentException("user not in voice channel");
			}
			IAudioClient client = await channel.ConnectAsync();

				//client.StreamCreated += async (id, stream) => {
				//	const int bitRate = 92000;
				//	WaveFormat format=new WaveFormat(bitRate/16,16,1);
					
				//	await using CueWaveFileWriter writer = new CueWaveFileWriter("D:\\Downloads\\out.wav", format);
				//	CancellationTokenSource source=new CancellationTokenSource(30000);
				//	try { 
				//	await stream.CopyToAsync(writer,source.Token);
				//	}
				//	catch (Exception e) {
					
				//	}
				//	// ReSharper disable once MethodSupportsCancellation
				//	await writer.FlushAsync();
				//};
					

			await using Mp3FileReader fileReader = new Mp3FileReader("D:\\Downloads\\test.mp3");
			{
				int bitrate = 92000;
				AudioOutStream dstream = client.CreatePCMStream(AudioApplication.Mixed, bitrate);
				await fileReader.CopyToAsync(dstream);
			}
		}
	}
}
