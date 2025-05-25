using MessagePack;
using Microsoft.EntityFrameworkCore;
using Server.Crypto;
using Server.Data;
using Server.Models.Dto.Account.Create;
using Server.Models.Entitys;
using Server.Services.Validation;

namespace Server.Services
{
    public interface IAccountService
    {
        Task<bool> CreateAccountAsync(byte[] requestSignature, byte[] rawJson);
    }
    public class AccountService(
        AppDbContext dbContext,
        IProofOfWorkService proofOfWorkService,
        IHCaptchaService hCaptchaService,
        ITimestampValidator timestampValidator,
        IMlDsaKeyVerifier mlDsaKeyVerifier) : IAccountService
    {
        public async Task<bool> CreateAccountAsync(byte[] requestSignature, byte[] rawData)
        {
            try
            {
                var request = MessagePackSerializer.Deserialize<CreateAccountRequest>(rawData);

                if (!timestampValidator.IsValid(request.Timestamp))
                    return false;

                if (!await hCaptchaService.VerifyAsync(request.ChaptchaToken))
                    return false;

                if (!(request.Device.PreKeys.Length == 50))
                    return false;

                if (!await mlDsaKeyVerifier.VerifyAsync(request.Device.SPK, rawData, requestSignature))
                    return false;

                if (!await proofOfWorkService.VerifyAsync(request.Id, request.ProofOfWork, request.Nonce, Convert.ToBase64String(request.Device.SPK)))
                    return false;

                if (await dbContext.Accounts.AnyAsync(a => a.Id == request.Id))
                    return false;

                Account newAccount = new()
                {
                    Id = request.Id,
                    Name = request.Username ?? throw new ArgumentNullException(),
                    Devices = []
                };
                newAccount.Devices.Add(new Device()
                {
                    Id = request.Device.Id,
                    Name = request.Device.Name,
                    SPK = request.Device.SPK,
                    Signature = request.Device.Signature,
                    PreKeys = []
                });
                foreach (var preKey in request.Device.PreKeys)
                {
                    newAccount.Devices[0].PreKeys.Add(new PreKey()
                    {
                        Id = preKey.Id,
                        PK = preKey.PK,
                        PKSignature = preKey.PKSignature
                    });
                }
                await dbContext.Accounts.AddAsync(newAccount);
                await dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
