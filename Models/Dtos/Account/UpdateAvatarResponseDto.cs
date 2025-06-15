namespace ArqanumServer.Models.Dtos.Account
{
    public class UpdateAvatarResponseDto
    {
        public string AvatarUrl { get; set; }

        public long Version { get; set; }

        public long Timestamp { get; set; }
    }
}
