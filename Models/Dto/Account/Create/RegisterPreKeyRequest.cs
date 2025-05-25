using MessagePack;

namespace Server.Models.Dto.Account.Create
{
    [MessagePackObject]
    internal class RegisterPreKeyRequest
    {
        [Key(0)] public string Id { get; set; }
        [Key(1)] public byte[] PK { get; set; }
        [Key(2)] public byte[] PKSignature { get; set; }
    }

}
