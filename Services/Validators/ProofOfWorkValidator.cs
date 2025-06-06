using ArqanumServer.Crypto;

namespace ArqanumServer.Services.Validators
{
    public interface IProofOfWorkValidator
    {
        Task<bool> CheckProofAsync(string accountId, string hashProof, string nonce, string PK);
    }
    public class ProofOfWorkValidator(IShakeGenerator shakeGenerator) : IProofOfWorkValidator
    {
        public async Task<bool> CheckProofAsync(string accountId, string proof, string proofNonce, string publicKey)
        {
            try
            {
                string input = publicKey + proofNonce;
                string recomputedHash = Convert.ToBase64String(await shakeGenerator.ComputeHash128(input)).ToLowerInvariant();

                if (recomputedHash != proof)
                    return false;

                if (!recomputedHash.StartsWith("000"))
                    return false;

                var computeId = Convert.ToBase64String(await shakeGenerator.ComputeHash256(Convert.FromBase64String(publicKey), 64));
                if (computeId != accountId)
                    return false;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
