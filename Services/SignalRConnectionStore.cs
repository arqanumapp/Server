using Newtonsoft.Json;
using System.Text.Json;

namespace ArqanumServer.Services
{
    public interface ISignalRConnectionStore
    {
        Task<bool> AddConnectionAsync(string accountId, string connectionId);

        Task<string?> GetConnectionAsync(string accountId);

        Task<bool> RemoveConnectionAsync(string connectionId);

        Task<string?> GetAccountIdByConnectionAsync(string connectionId);

        Task<bool> RemoveAllConnectionsAsync(string accountId);
    }

    public class SignalRConnectionStore : ISignalRConnectionStore
    {
        private readonly HttpClient _httpClient;
        private const string AccountKeyPrefix = "signalr:acct:";
        private const string ConnKeyPrefix = "signalr:conn:";

        public SignalRConnectionStore(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("Upstash");
        }

        private HttpRequestMessage CreateRedisCommand(params string[] args)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "")
            {
                Content = JsonContent.Create(args)
            };
            return req;
        }

        public async Task<bool> AddConnectionAsync(string accountId, string connectionId)
        {
            var sadd = CreateRedisCommand("SADD", AccountKeyPrefix + accountId, connectionId);
            var set = CreateRedisCommand("SET", ConnKeyPrefix + connectionId, accountId);

            var r1 = await _httpClient.SendAsync(sadd);
            var r2 = await _httpClient.SendAsync(set);
            return r1.IsSuccessStatusCode && r2.IsSuccessStatusCode;
        }

        public async Task<string?> GetConnectionAsync(string accountId)
        {
            var req = CreateRedisCommand("SMEMBERS", AccountKeyPrefix + accountId);
            var resp = await _httpClient.SendAsync(req);
            if (!resp.IsSuccessStatusCode) return null;

            var json = await resp.Content.ReadAsStringAsync();
            var parsed = JsonConvert.DeserializeObject<RedisArrayResponse>(json);
            return parsed?.Result?.FirstOrDefault();
        }

        public async Task<string?> GetAccountIdByConnectionAsync(string connectionId)
        {
            var req = CreateRedisCommand("GET", ConnKeyPrefix + connectionId);
            var resp = await _httpClient.SendAsync(req);
            if (!resp.IsSuccessStatusCode) return null;

            var json = await resp.Content.ReadAsStringAsync();
            var parsed = JsonConvert.DeserializeObject<RedisStringResponse>(json);
            return parsed?.Result;
        }


        public async Task<bool> RemoveConnectionAsync(string connectionId)
        {
            var getAccount = CreateRedisCommand("GET", ConnKeyPrefix + connectionId);
            var getResp = await _httpClient.SendAsync(getAccount);
            if (!getResp.IsSuccessStatusCode) return false;

            var json = await getResp.Content.ReadAsStringAsync();
            var parsed = JsonConvert.DeserializeObject<RedisStringResponse>(json);
            var accountId = parsed?.Result;
            if (string.IsNullOrWhiteSpace(accountId)) return false;

            var srem = CreateRedisCommand("SREM", AccountKeyPrefix + accountId, connectionId);
            var del = CreateRedisCommand("DEL", ConnKeyPrefix + connectionId);

            var r1 = await _httpClient.SendAsync(srem);
            var r2 = await _httpClient.SendAsync(del);

            return r1.IsSuccessStatusCode && r2.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveAllConnectionsAsync(string accountId)
        {
            var membersResp = await _httpClient.SendAsync(CreateRedisCommand("SMEMBERS", AccountKeyPrefix + accountId));
            if (!membersResp.IsSuccessStatusCode) return false;

            var json = await membersResp.Content.ReadAsStringAsync();
            var parsed = JsonConvert.DeserializeObject<RedisArrayResponse>(json);
            var connectionIds = parsed?.Result;
            if (connectionIds == null || connectionIds.Count == 0) return true;

            var tasks = new List<Task<bool>>();

            foreach (var connectionId in connectionIds)
            {
                tasks.Add(RemoveConnectionAsync(connectionId));
            }

            var results = await Task.WhenAll(tasks);

            return results.All(r => r);
        }


        private class RedisStringResponse
        {
            public string? Result { get; set; }
        }

        private class RedisArrayResponse
        {
            [JsonProperty("result")]
            public List<string>? Result { get; set; }
        }
    }
}