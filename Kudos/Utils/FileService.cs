#region
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
#endregion

namespace Kudos.Utils {
	public class FileService {
		private const string ApplicationFolderName = "KudosData";
		private const string JsonFileEnding = ".json";

		private static readonly JsonSerializerSettings JsonSettings = new() {
			TypeNameHandling = TypeNameHandling.Auto, Converters = { new JsonIEmoteConverter(), new JsonIMessageChannelConverter() }
		};

		private readonly object _logFile = new();

		public string ApplicationFolderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationFolderName);

		private static string ExecutablePath => AppDomain.CurrentDomain.BaseDirectory;
		public static FileService Instance { get; } = new();

		public Dictionary<LogType, string> Logs { get; } = new() { { LogType.Normal, "" }, { LogType.Access, "access-" }, { LogType.Login, "login-" } };

		public AsyncThreadsafeFileSyncedDictionary<string, string> Settings { get; } = new("settings");
		static FileService() { }

		private FileService() {
			if (!Directory.Exists(ApplicationFolderPath)) {
				Directory.CreateDirectory(ApplicationFolderPath);
			}
		}

		public void DeleteLog(LogType logType = LogType.Normal) {
			string fileName = Path.Combine(ApplicationFolderPath, Logs[logType] + "log.txt");

			lock (_logFile) {
				File.Delete(fileName);
			}
		}

		public void DeleteRunningLog() {
			string fileName = Path.Combine(ExecutablePath, "runningLog.txt");
			File.Delete(fileName);
		}

		public void Log(string message, LogType logType = LogType.Normal) {
			string fileName = Path.Combine(ApplicationFolderPath, Logs[logType] + "log.txt");
			message = $"{DateTime.UtcNow}: {message}\n";
			lock (_logFile) {
				File.AppendAllText(fileName, message);
			}
		}

		public async Task<T> ReadJsonFromFile<T>(string fileName)
			where T : new() {
			return await Task.Run(() => {
				fileName = Path.Combine(ApplicationFolderPath, fileName + JsonFileEnding);
				if (!File.Exists(fileName)) {
					return new T();
				}
				string json = File.ReadAllText(fileName);
				T val = JsonConvert.DeserializeObject<T>(json, JsonSettings);
				return val ?? new T();
			});
		}

		public string ReadLog(LogType logType = LogType.Normal) {
			string fileName = Path.Combine(ApplicationFolderPath, Logs[logType] + "log.txt");
			try {
				lock (_logFile) {
					return File.ReadAllText(fileName);
				}
			}
			catch (FileNotFoundException) {
				return "not found";
			}
		}

		public string ReadRunningLog() {
			string fileName = Path.Combine(ExecutablePath, "runningLog.txt");
			try {
				return File.ReadAllText(fileName);
			}
			catch (FileNotFoundException) {
				return "not found";
			}
		}

		public async Task SaveJsonToFile<T>(string fileName, T content) {
			await Task.Run(() => {
				fileName = Path.Combine(ApplicationFolderPath, fileName + JsonFileEnding);
				string json = JsonConvert.SerializeObject(content, JsonSettings);

				File.WriteAllText(fileName, json);
			});
		}

		public void WriteFile(string name, string content) {
			File.WriteAllText(Path.Combine(ApplicationFolderPath, name), content);
		}

		public enum LogType {
			Normal,
			Access,
			Login
		}
	}
}
