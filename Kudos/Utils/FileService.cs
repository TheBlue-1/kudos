#region
using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
#endregion

namespace Kudos.Utils {
	public class FileService {
		private const string ApplicationFolderName = "KudosData";

		private static string ApplicationFolderPath =>
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationFolderName);
		public static FileService Instance { get; } = new FileService();

		public AsyncFileSyncedDictionary<string, string> Settings { get; } = new AsyncFileSyncedDictionary<string, string>("settings");
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
				fileName = Path.Combine(ApplicationFolderPath, fileName);
				if (!File.Exists(fileName)) {
					return new T();
				}
				string json = File.ReadAllText(fileName);
				T val = JsonConvert.DeserializeObject<T>(json);
				return val ?? new T();
			});
		}

		public async Task SaveJsonToFile<T>(string fileName, T content) {
			await Task.Run(() => {
				fileName = Path.Combine(ApplicationFolderPath, fileName);
				string json = JsonConvert.SerializeObject(content);
				File.WriteAllText(fileName, json);
			});
		}
	}
}
