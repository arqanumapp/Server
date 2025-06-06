using ArqanumServer.Crypto;
using ArqanumServer.Data;
using ArqanumServer.Models.Dtos.Account;
using ArqanumServer.Models.Entities;
using ArqanumServer.Services.Validators;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace ArqanumServer.Services
{
    public interface IAccountService
    {
        Task<bool> CreateAccountAsync(byte[] requestSignature, byte[] rawJson);

        Task<bool> IsUsernameTakenAsync(string username);
    }
    public class AccountService(AppDbContext dbContext, IProofOfWorkValidator proofOfWorkService, IHCaptchaValidator hCaptchaService, ITimestampValidator timestampValidator, IMlDsaKeyVerifier mlDsaKeyVerifier) : IAccountService
    {
        public async Task<bool> CreateAccountAsync(byte[] requestSignature, byte[] rawData)
        {
            try
            {
                var request = MessagePackSerializer.Deserialize<CreateAccountDto>(rawData);

                if (!timestampValidator.IsValid(request.Timestamp))
                    return false;

                if (!await hCaptchaService.VerifyAsync(request.ChaptchaToken))
                    return false;

                if (await dbContext.Accounts.AnyAsync(a => a.Id == request.AccountId))
                    return false;

                if (!await mlDsaKeyVerifier.VerifyAsync(request.SignaturePublicKey, rawData, requestSignature))
                    return false;

                if (!await proofOfWorkService.CheckProofAsync(request.AccountId, request.ProofOfWork, request.ProofOfWorkNonce, Convert.ToBase64String(request.SignaturePublicKey)))
                    return false;

                Account newAccount = new()
                {
                    Id = request.AccountId,
                    Username = request.Username,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    SignaturePublicKey = request.SignaturePublicKey,
                };

                await dbContext.Accounts.AddAsync(newAccount);
                await dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> IsUsernameTakenAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username must not be null or empty.", nameof(username));

            return await dbContext.Accounts
                .AsNoTracking()
                .AnyAsync(a => a.Username == username);
        }
    }
}
