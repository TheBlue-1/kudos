﻿#region
using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
#endregion

namespace Kudos.Utils {
	public class FileService {
		private const string ApplicationFolderName = "KudosData";
		private const string JsonFileEnding = ".json";

		private static readonly JsonSerializerSettings JsonSettings =
			new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, Converters = { new JsonIEmoteConverter() } };

		public string ApplicationFolderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationFolderName);
		public static FileService Instance { get; } = new FileService();

		public AsyncThreadsafeFileSyncedDictionary<string, string> Settings { get; } = new AsyncThreadsafeFileSyncedDictionary<string, string>("settings");
		static FileService() { }

		private FileService() {
			if (!Directory.Exists(ApplicationFolderPath)) {
				Directory.CreateDirectory(ApplicationFolderPath);
			}
		}

		public void Log(string message) {
			string fileName = Path.Combine(ApplicationFolderPath, "log.txt");
			message = $"{DateTime.Now}: {message}\n";
			File.AppendAllText(fileName, message);
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
	}
}
