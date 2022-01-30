using System.Net.Http;
using System.Threading.Tasks;

namespace Kudos.Utils {

    public class UptimeHeartbeatClient {
        private HttpClient HttpClient { get; } = new();

        private AsyncThreadsafeFileSyncedDictionary<string, string> Settings { get; } = FileService.Instance.Settings;

        public async Task Call() {
            if (!Settings.ContainsKey("heartbeat_uri")) return;
            try {
                await HttpClient.GetAsync(Settings["heartbeat_uri"]);
            } catch {
                // heartbeat failed
            }
        }
    }
}