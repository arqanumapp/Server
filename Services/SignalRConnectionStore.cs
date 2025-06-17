using StackExchange.Redis;

namespace ArqanumServer.Services;

public interface ISignalRConnectionStore
{
    Task<bool> AddConnectionAsync(string accountId, string connectionId);

    Task<IReadOnlyCollection<string>> GetConnectionsAsync(string accountId);

    Task<bool> RemoveConnectionAsync(string connectionId);

    Task<string?> GetAccountIdByConnectionAsync(string connectionId);
}

public class SignalRConnectionStore(IConnectionMultiplexer connectionMultiplexer) : ISignalRConnectionStore
{
    private readonly IDatabase _db = connectionMultiplexer.GetDatabase();

    private const string AccountKeyPrefix = "signalr:acct:";
    private const string ConnKeyPrefix = "signalr:conn:";


    private RedisValue Serialize(string value) => value;
    private string Deserialize(RedisValue value) => value;

    public async Task<bool> AddConnectionAsync(string accountId, string connectionId)
    {
        if (string.IsNullOrWhiteSpace(accountId) || string.IsNullOrWhiteSpace(connectionId))
            return false;

        var accountKey = AccountKeyPrefix + accountId;
        var connKey = ConnKeyPrefix + connectionId;

        var batch = _db.CreateBatch();
        var t1 = batch.SetAddAsync(accountKey, connectionId);
        var t2 = batch.StringSetAsync(connKey, accountId);
        batch.Execute();
        await Task.WhenAll(t1, t2);
        return t1.Result && t2.Result;
    }

    public async Task<IReadOnlyCollection<string>> GetConnectionsAsync(string accountId)
    {
        var accountKey = AccountKeyPrefix + accountId;
        var values = await _db.SetMembersAsync(accountKey);

        return values.Select(x => (string)x).ToArray();
    }

    public async Task<bool> RemoveConnectionAsync(string connectionId)
    {
        var connKey = ConnKeyPrefix + connectionId;
        var accountId = await _db.StringGetAsync(connKey);
        if (accountId.IsNullOrEmpty)
            return false;

        var accountKey = AccountKeyPrefix + accountId;

        var batch = _db.CreateBatch();
        var t1 = batch.SetRemoveAsync(accountKey, connectionId);
        var t2 = batch.KeyDeleteAsync(connKey);
        batch.Execute();
        await Task.WhenAll(t1, t2);
        return t1.Result;
    }

    public async Task<string?> GetAccountIdByConnectionAsync(string connectionId)
    {
        var connKey = ConnKeyPrefix + connectionId;
        var val = await _db.StringGetAsync(connKey);
        return val.IsNullOrEmpty ? null : val.ToString();
    }
}
