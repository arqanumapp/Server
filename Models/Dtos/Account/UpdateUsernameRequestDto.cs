using MessagePack;

namespace ArqanumServer.Models.Dtos.Account
{
    [MessagePackObject]
    public class UpdateUsernameRequestDto
    {
        [Key(0)] public string AccountId { get; set; }

        [Key(1)] public string NewUsername { get; set; }

        [Key(2)] public long Timestamp { get; set; }
    }
}
