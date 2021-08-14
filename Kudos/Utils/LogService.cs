#region
using System;
using System.Collections.Generic;
using Google.Api;
using Google.Cloud.Logging.Type;
using Google.Cloud.Logging.V2;
using Kudos.Extensions;
#endregion

namespace Kudos.Utils {
	public class LogService {
		private const string LogId = "kudos-main";
		private const string ProjectId = "kudos-322808";
		private LoggingServiceV2Client Client { get; }
		public static LogService Instance { get; } = new();

		private LogName LogName { get; } = new(ProjectId, LogId);
		private Dictionary<LogType, string> Logs { get; } = new() {
			{ LogType.Main, "main" }, { LogType.Access, "access" }, { LogType.Running, "running" }, { LogType.Login, "login" }
		};
		static LogService() { }

		private LogService() {
			string credentials = FileService.Instance.ReadGoogleApiAuth();
			if (credentials != null) {
				Client = new LoggingServiceV2ClientBuilder { JsonCredentials = credentials }.Build();
			} else {
				Console.WriteLine(
					"No google credentials given. Logging-Mode Debug. Logging to Console. (if you want to change this add the credentials json file to the KudosData folder)");
			}
		}

		public void Log(string text, LogType logType, LogSeverity severity) {
			if (Client == null) {
				Console.WriteLine($"{DateTime.Now} {logType} {severity} {text}");
				return;
			}
			new Action(() => {
				LogEntry logEntry = new() { LogNameAsLogName = LogName, Severity = severity, TextPayload = text };
				MonitoredResource resource = new() { Type = "global" };
				Dictionary<string, string> labels = new() { { "logType", Logs[logType] } };
				Client.WriteLogEntries(LogName, resource, labels, new[] { logEntry });
			}).RunAsyncSave();
		}

		public enum LogType {
			Main,
			Access,
			Login,
			Running
		}
	}
}
