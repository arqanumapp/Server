namespace ArqanumServer.Services.Validators
{
    public interface ITimestampValidator
    {
        bool IsValid(long timestamp);
    }

    public class TimestampValidator(long maxSkewSeconds = 30) : ITimestampValidator
    {
        public bool IsValid(long timestamp)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return Math.Abs(now - timestamp) <= maxSkewSeconds;
        }
    }
}
