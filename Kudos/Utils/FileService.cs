#region
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Newtonsoft.Json;
#endregion

namespace Kudos.Utils {
	public class FileService {
		private const string ApplicationFolderName = "Kudos";

		private string ApplicationFolderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationFolderName);
		public static FileService Instance { get; } = new FileService();

		public AsyncFileSyncedDictionary<string, string> Settings { get; } =
			new AsyncFileSyncedDictionary<string, string>("settings", SerializationFormat.Json);
		static FileService() { }

		private FileService() {
			if (!Directory.Exists(ApplicationFolderPath)) {
				Directory.CreateDirectory(ApplicationFolderPath);
			}
		}

		private object ByteArrayToObject(byte[] arr) {
			using MemoryStream memoryStream = new MemoryStream();
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			try {
				memoryStream.Write(arr, 0, arr.Length);
				memoryStream.Seek(0, SeekOrigin.Begin);
				object obj = binaryFormatter.Deserialize(memoryStream);
				return obj;
			}
			catch (Exception) {
				return null;
			}
		}

		private byte[] ObjectToByteArray(object obj) {
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using MemoryStream memoryStream = new MemoryStream();
			binaryFormatter.Serialize(memoryStream, obj);
			return memoryStream.ToArray();
		}

		public async Task<T> ReadFromFile<T>(string fileName)
			where T : new() {
			return await Task.Run(() => {
				fileName = Path.Combine(ApplicationFolderPath, fileName);
				if (File.Exists(fileName)) {
					byte[] byteContent = File.ReadAllBytes(fileName);
					T val = (T)ByteArrayToObject(byteContent);
					if (val != null) {
						return val;
					}
				}
				return new T();
			});
		}

		public async Task<T> ReadJsonFromFile<T>(string fileName)
			where T : new() {
			return await Task.Run(() => {
				fileName = Path.Combine(ApplicationFolderPath, fileName);
				if (File.Exists(fileName)) {
					string json = File.ReadAllText(fileName);
					T val = JsonConvert.DeserializeObject<T>(json);
					if (val != null) {
						return val;
					}
				}
				return new T();
			});
		}

		public async Task SaveJsonToFile<T>(string fileName, T content) {
			await Task.Run(() => {
				fileName = Path.Combine(ApplicationFolderPath, fileName);
				string json = JsonConvert.SerializeObject(content);
				File.WriteAllText(fileName, json);
			});
		}

		public async Task SaveToFile<T>(string fileName, T content) {
			await Task.Run(() => {
				fileName = Path.Combine(ApplicationFolderPath, fileName);
				byte[] byteContent = ObjectToByteArray(content);
				File.WriteAllBytes(fileName, byteContent);
			});
		}
	}
}
