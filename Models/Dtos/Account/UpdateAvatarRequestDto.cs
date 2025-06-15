using MessagePack;

namespace ArqanumServer.Models.Dtos.Account
{
    [MessagePackObject]
    public class UpdateAvatarRequestDto
    {
        [Key(0)] public string AccountId { get; set; }

        [Key(1)] public byte[] AvatarData { get; set; }

        [Key(2)] public string Format { get; set; }

        [Key(3)] public long Timestamp { get; set; }
    }
}
