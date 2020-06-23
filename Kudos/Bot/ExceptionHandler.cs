﻿#region
using System;
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
			Exception exception = Exception;
			while (true) {
				if (exception is IKudosException kudosException) {
					Messaging.Instance.Message(Channel, kudosException.UserMessage);
					return;
				}
				if (exception.InnerException == null) {
					break;
				}
				exception = exception.InnerException;
			}

			Messaging.Instance.Message(Channel, "unknown error occured");
			FileService.Instance.Log(Exception.ToString());
		}

		private void SendInternalError() {
			Messaging.Instance.Message(Channel, "An internal error occured");
			FileService.Instance.Log($"error while error handling: \n{Exception}");
		}
	}
}
