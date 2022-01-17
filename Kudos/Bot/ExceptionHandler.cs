#region

using Discord;
using Kudos.Bot.Modules;
using Kudos.Exceptions;
using Kudos.Utils;
using System;
using System.Threading.Tasks;
using LogSeverity = Google.Cloud.Logging.Type.LogSeverity;

#endregion

namespace Kudos.Bot {

    public class ExceptionHandler {
        private static readonly TimeSpan ExceptionMessageLifeTime = new(0, 0, 15);
        private IMessageChannel Channel { get; }
        private Exception Exception { get; }

        public ExceptionHandler(Exception exception, IMessageChannel channel) {
            Exception = exception;
            Channel = channel;
        }

        public async void Handle(bool sendMessages) {
            try {
                await HandleException(sendMessages);
            } catch (Exception) {
                try {
                    await SendInternalError(sendMessages);
                } catch (Exception e) {
                    try {
                        LogService.Instance.Log($"Exception could not be handled: {e.Message}\n{e}", LogService.LogType.Main, LogSeverity.Error);
                    } catch (Exception) {
                        //ignored (couldn't send internal error msg, nothing more kudos can do)
                    }
                }
            }
        }

        private async Task HandleException(bool sendMessages) {
            Exception exception = Exception;
            while (true) {
                if (exception is AggregateException) {
                    if (exception.Message == "One or more errors occurred. (The operation has timed out.)") {
                        exception = new KudosInvalidOperationException(
                            "A problem with the communication to discord occurred (command execution was stopped)!\n Maybe you requested too much (for example deleting hundreds of messages), if you did please stop it! (if you use features in a way they aren't meant to use, I have to lock them)",
                            "connection timeout to discord (maybe too much traffic)", new TimeSpan(0, 0, 30));
                    }
                }
                if (exception is TaskCanceledException) {
                    LogService.Instance.Log($"task got cancelled:\n{exception}", LogService.LogType.Main, LogSeverity.Notice);
                }
                if (exception is IKudosException kudosException) {
                    LogService.Instance.Log($"{kudosException.UserMessage} ({kudosException.Message})", LogService.LogType.Main, LogSeverity.Warning);
                    if (!sendMessages) {
                        return;
                    }
                    string message = kudosException.UserMessage;

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (Program.Debug) {
                        message += $"\n Log: {kudosException.Message}";
                    }
                    await Messaging.Instance.SendExpiringMessage(Channel, message, kudosException.Lifetime ?? ExceptionMessageLifeTime);
                    return;
                }

                if (exception.InnerException == null) {
                    break;
                }
                exception = exception.InnerException;
            }
            if (sendMessages) {
                await Messaging.Instance.SendExpiringMessage(Channel, "unknown error occurred", ExceptionMessageLifeTime);
            }
            LogService.Instance.Log(Exception.ToString(), LogService.LogType.Main, LogSeverity.Critical);
        }

        private async Task SendInternalError(bool sendMessages) {
            if (sendMessages) {
                await Messaging.Instance.SendExpiringMessage(Channel, "An internal error occurred", ExceptionMessageLifeTime);
            }
            LogService.Instance.Log($"error while error handling: \n{Exception}", LogService.LogType.Main, LogSeverity.Critical);
        }
    }
}