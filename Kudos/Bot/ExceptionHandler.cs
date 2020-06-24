#region
using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Kudos.Bot.Modules;
using Kudos.Exceptions;
using Kudos.Utils;
#endregion

namespace Kudos.Bot {
	public class ExceptionHandler {
		private ISocketMessageChannel Channel { get; }
		private Exception Exception { get; }

		public ExceptionHandler(Exception exception, ISocketMessageChannel channel) {
			Exception = exception;
			Channel = channel;
		}

		public async void Handle() {
			try {
				await HandleException();
			}
			catch (Exception) {
				try {
					await SendInternalError();
				}
				catch (Exception) {
					//ignored (couldn't send internal error msg, nothing more kudos can do)
				}
			}
		}

		private async Task HandleException() {
			Exception exception = Exception;
			while (true) {
				if (exception is IKudosException kudosException) {
					await Messaging.Instance.SendMessage(Channel, kudosException.UserMessage);
					return;
				}
				if (exception.InnerException == null) {
					break;
				}
				exception = exception.InnerException;
			}

			await Messaging.Instance.SendMessage(Channel, "unknown error occured");
			FileService.Instance.Log(Exception.ToString());
		}

		private async Task SendInternalError() {
			await Messaging.Instance.SendMessage(Channel, "An internal error occured");
			FileService.Instance.Log($"error while error handling: \n{Exception}");
		}
	}
}
