#region
using System;
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

		public string ApplicationFolderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationFolderName);

		public static FileService Instance { get; } = new();

		public AsyncThreadsafeFileSyncedDictionary<string, string> Settings { get; } = new("settings");
		static FileService() { }

		private FileService() {
			if (!Directory.Exists(ApplicationFolderPath)) {
				Directory.CreateDirectory(ApplicationFolderPath);
			}
		}

		internal string ReadGoogleApiAuth() {
			string fileName = Path.Combine(ApplicationFolderPath, "google-api-auth" + JsonFileEnding);
			return !File.Exists(fileName) ? null : File.ReadAllText(fileName);
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
