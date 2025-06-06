using Org.BouncyCastle.Crypto.Digests;
using System.Text;

namespace ArqanumServer.Crypto
{
    public interface IShakeGenerator
    {
        Task<byte[]> ComputeHash128(string input);
        Task<byte[]> ComputeHash256(byte[] input, int outputLength = 32);
    }
    public class ShakeGenerator : IShakeGenerator
    {
        public async Task<byte[]> ComputeHash128(string input)
        {
            return await Task.Run(() =>
            {
                var shake = new ShakeDigest(128);
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                shake.BlockUpdate(inputBytes, 0, inputBytes.Length);
                byte[] output = new byte[32];
                shake.DoFinal(output, 0);
                return output;
            });
        }

        public async Task<byte[]> ComputeHash256(byte[] input, int outputLength = 32)
        {
            return await Task.Run(() =>
            {
                var digest = new ShakeDigest(256);
                digest.BlockUpdate(input, 0, input.Length);
                byte[] output = new byte[outputLength];
                digest.DoFinal(output, 0);
                return output;
            });
        }
    }
}
