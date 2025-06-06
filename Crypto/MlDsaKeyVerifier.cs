using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

namespace ArqanumServer.Crypto
{
    public interface IMlDsaKeyVerifier
    {
        Task<bool> VerifyAsync(byte[] publicKeyBytes, byte[] message, byte[] signature);
    }
    public class MlDsaKeyVerifier : IMlDsaKeyVerifier
    {
        public async Task<bool> VerifyAsync(byte[] publicKeyBytes, byte[] message, byte[] signature)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var publicKey = MLDsaPublicKeyParameters.FromEncoding(MLDsaParameters.ml_dsa_87, publicKeyBytes);
                    var verifier = new MLDsaSigner(MLDsaParameters.ml_dsa_87, false);
                    verifier.Init(false, publicKey);
                    verifier.BlockUpdate(message, 0, message.Length);
                    return verifier.VerifySignature(signature);
                }
                catch (Exception ex)
                {
                    return false;
                }
            });
        }
    }
}
