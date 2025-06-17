using MessagePack;

namespace ArqanumServer.Models.Dtos.Hub
{
    [MessagePackObject]
    public class HubConnectionAuth
    {
        [Key(0)] public string AccountId { get; set; }

        [Key(1)] public long Timestamp { get; set; }

        [Key(2)] public byte[] RandomBytes { get; set; }
    }
}
