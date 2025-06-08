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
        Task<(bool IsComplete, string? AvatarUrl)> CreateAccountAsync(byte[] requestSignature, byte[] rawData);

        Task<UsernameAvailabilityResponseDto> IsUsernameTakenAsync(string username);
    }
    public class AccountService(AppDbContext dbContext, IProofOfWorkValidator proofOfWorkService, IHCaptchaValidator hCaptchaService, ITimestampValidator timestampValidator, IMlDsaKeyVerifier mlDsaKeyVerifier, IAvatarService avatarService) : IAccountService
    {
        public async Task<(bool IsComplete, string? AvatarUrl)> CreateAccountAsync(byte[] requestSignature, byte[] rawData)
        {
            try
            {
                var request = MessagePackSerializer.Deserialize<CreateAccountDto>(rawData);

                if (!timestampValidator.IsValid(request.Timestamp))
                    return (false, null);

                if (!await hCaptchaService.VerifyAsync(request.CaptchaToken))
                    return (false, null);

                if (await dbContext.Accounts.AnyAsync(a => a.Id == request.AccountId))
                    return (false, null);

                if (!await mlDsaKeyVerifier.VerifyAsync(request.SignaturePublicKey, rawData, requestSignature))
                    return (false, null);

                if (!await proofOfWorkService.CheckProofAsync(request.AccountId, request.ProofOfWork, request.ProofOfWorkNonce, Convert.ToBase64String(request.SignaturePublicKey)))
                    return (false, null);

                var initial = !string.IsNullOrEmpty(request.Username) ? request.Username[0] : 'A';

                var defaultAvatar = await avatarService.GenerateAvatarStream(initial);

                var defaultAvatarUrl = await avatarService.SaveAvatarAsync(defaultAvatar, "png");

                Account newAccount = new()
                {
                    Id = request.AccountId,
                    Username = request.Username,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    SignaturePublicKey = request.SignaturePublicKey,
                    AvatarUrl = defaultAvatarUrl,
                };

                await dbContext.Accounts.AddAsync(newAccount);
                await dbContext.SaveChangesAsync();
                return (true, defaultAvatarUrl);
            }
            catch (Exception ex)
            {
                return (false, null);
            }
        }

        public async Task<UsernameAvailabilityResponseDto> IsUsernameTakenAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username must not be null or empty.", nameof(username));

            var exists = await dbContext.Accounts
                .AsNoTracking()
                .AnyAsync(a => a.Username == username);

            return new UsernameAvailabilityResponseDto { Available = !exists };
        }
    }
}
