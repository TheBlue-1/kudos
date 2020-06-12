#region
using System;
using Discord.WebSocket;
using Kudos.Exceptions;
#endregion

namespace Kudos.Bot {
	public class ExceptionHandler {
		private ISocketMessageChannel Channel { get; }
		private Exception Exception { get; }

		public ExceptionHandler(Exception exception, ISocketMessageChannel channel) {
			Exception = exception;
			Channel = channel;
		}

		public void Handle() {
			try {
				HandleException();
			}
			catch (Exception) {
				try {
					SendInternalError();
				}
				catch (Exception) {
					//ignored (couldn't send internal error msg, nothing more kudos can do)
				}
			}
		}

		private void HandleException() {
			if (Exception is IKudosException kudosException) {
				Messaging.Instance.Message(Channel, kudosException.UserMessage);
				return;
			}
			Messaging.Instance.Message(Channel, "unknown error occured");

			//TODO handle normal exceptions
		}

		private void SendInternalError() {
			Messaging.Instance.Message(Channel, "An internal error occured");
		}
	}
}
