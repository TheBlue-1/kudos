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

		public async void Handle(bool sendMessages) {
			try {
				await HandleException(sendMessages);
			}
			catch (Exception) {
				try {
					await SendInternalError(sendMessages);
				}
				catch (Exception) {
					//ignored (couldn't send internal error msg, nothing more kudos can do)
				}
			}
		}

		private async Task HandleException(bool sendMessages) {
			Exception exception = Exception;
			while (true) {
				if (exception is IKudosException kudosException) {
					if (!sendMessages) {
						return;
					}
					string message = kudosException.UserMessage;

					// ReSharper disable once ConditionIsAlwaysTrueOrFalse
					if (Program.Debug) {
						message += $"\n Log: {kudosException.Message}";
					}
					await Messaging.Instance.SendMessage(Channel, message);
					return;
				}
				if (exception.InnerException == null) {
					break;
				}
				exception = exception.InnerException;
			}
			if (sendMessages) {
				await Messaging.Instance.SendMessage(Channel, "unknown error occured");
			}
			FileService.Instance.Log(Exception.ToString());
		}

		private async Task SendInternalError(bool sendMessages) {
			if (sendMessages) {
				await Messaging.Instance.SendMessage(Channel, "An internal error occured");
			}
			FileService.Instance.Log($"error while error handling: \n{Exception}");
		}
	}
}
