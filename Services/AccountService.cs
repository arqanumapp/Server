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

        Task<UsernameAvailabilityResponseDto> IsUsernameTakenAsync(UsernameAvailabilityRequestDto dto);

        Task<(UpdateFullNameResponseDto? Response, bool IsComplete)> UpdateFullNameAsync(byte[] requestSignature, byte[] rawData);

        Task<(UpdateUsernameResponseDto? Response, bool IsComplete)> UpdateUsernameAsync(byte[] requestSignature, byte[] rawData);

        Task<(UpdateBioResponseDto? Response, bool IsComplete)> UpdateBioAsync(byte[] requestSignature, byte[] rawData);

        Task<(UpdateAvatarResponseDto? Response, bool IsComplete)> UpdateAvatar(byte[] requestSignature, byte[] rawData);
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

        public async Task<UsernameAvailabilityResponseDto> IsUsernameTakenAsync(UsernameAvailabilityRequestDto dto)
        {
            try
            {
                if (!timestampValidator.IsValid(dto.Timestamp))
                    throw new ArgumentException("Invalid timestamp.", nameof(dto.Timestamp));

                if (string.IsNullOrWhiteSpace(dto.Username))
                    throw new ArgumentException("Username must not be null or empty.", nameof(dto.Username));

                var exists = await dbContext.Accounts
                    .AsNoTracking()
                    .AnyAsync(a => a.Username == dto.Username);

                return new UsernameAvailabilityResponseDto { Available = !exists, Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() };
            }
            catch (Exception ex)
            {
                return new UsernameAvailabilityResponseDto { Available = false, Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() };
            }
        }

        public async Task<(UpdateFullNameResponseDto? Response, bool IsComplete)> UpdateFullNameAsync(byte[] requestSignature, byte[] rawData)
        {
            try
            {
                var request = MessagePackSerializer.Deserialize<UpdateFullNameRequestDto>(rawData);

                if (!timestampValidator.IsValid(request.Timestamp))
                    return (null, false);

                var accountPublicKeyBytes = await dbContext.Accounts
                    .AsNoTracking()
                    .Where(a => a.Id == request.AccountId)
                    .Select(a => a.SignaturePublicKey)
                    .FirstOrDefaultAsync() ?? throw new ArgumentException();

                if (!await mlDsaKeyVerifier.VerifyAsync(accountPublicKeyBytes, rawData, requestSignature))
                    throw new UnauthorizedAccessException("Invalid signature.");

                var account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId);

                if (account == null)
                    return (null, false);

                account.FirstName = request.FirstName;
                account.LastName = request.LastName;
                account.Version++;

                await dbContext.SaveChangesAsync();

                return (new() { Version = account.Version, Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() }, true);
            }
            catch (Exception ex)
            {
                return (null, false);
            }
        }

        public async Task<(UpdateUsernameResponseDto? Response, bool IsComplete)> UpdateUsernameAsync(byte[] requestSignature, byte[] rawData)
        {
            try
            {
                var request = MessagePackSerializer.Deserialize<UpdateUsernameRequestDto>(rawData);

                if (!timestampValidator.IsValid(request.Timestamp))
                    return (null, false);

                bool isTaken = await dbContext.Accounts
                    .AnyAsync(a => a.Username == request.NewUsername && a.Id != request.AccountId);

                if (isTaken)
                    return (null, false);

                var accountPublicKeyBytes = await dbContext.Accounts
                    .AsNoTracking()
                    .Where(a => a.Id == request.AccountId)
                    .Select(a => a.SignaturePublicKey)
                    .FirstOrDefaultAsync() ?? throw new ArgumentException();

                if (!await mlDsaKeyVerifier.VerifyAsync(accountPublicKeyBytes, rawData, requestSignature))
                    return (null, false);

                var account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId);

                if (account == null)
                    return (null, false);

                account.Username = request.NewUsername;
                account.Version++;

                await dbContext.SaveChangesAsync();

                return (new() { Version = account.Version, Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() }, true);
            }
            catch (Exception ex)
            {
                return (null, false);
            }
        }

        public async Task<(UpdateBioResponseDto? Response, bool IsComplete)> UpdateBioAsync(byte[] requestSignature, byte[] rawData)
        {
            try
            {
                var request = MessagePackSerializer.Deserialize<UpdateBioRequestDto>(rawData);

                if (!timestampValidator.IsValid(request.Timestamp))
                    return (null, false);

                var accountPublicKeyBytes = await dbContext.Accounts
                    .AsNoTracking()
                    .Where(a => a.Id == request.AccountId)
                    .Select(a => a.SignaturePublicKey)
                    .FirstOrDefaultAsync() ?? throw new ArgumentException();

                if (!await mlDsaKeyVerifier.VerifyAsync(accountPublicKeyBytes, rawData, requestSignature))
                    return (null, false);

                var account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId);

                if (account == null)
                    return (null, false);

                account.Bio = request.Bio;
                account.Version++;

                await dbContext.SaveChangesAsync();

                return (new() { Version = account.Version, Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() }, true);
            }
            catch (Exception ex)
            {
                return (null, false);
            }
        }

        public async Task<(UpdateAvatarResponseDto? Response, bool IsComplete)> UpdateAvatar(byte[] requestSignature, byte[] rawData)
        {
            try
            {
                var request = MessagePackSerializer.Deserialize<UpdateAvatarRequestDto>(rawData);

                if (!timestampValidator.IsValid(request.Timestamp))
                    return (null, false);

                var accountPublicKeyBytes = await dbContext.Accounts
                    .AsNoTracking()
                    .Where(a => a.Id == request.AccountId)
                    .Select(a => a.SignaturePublicKey)
                    .FirstOrDefaultAsync() ?? throw new ArgumentException();

                if (!await mlDsaKeyVerifier.VerifyAsync(accountPublicKeyBytes, rawData, requestSignature))
                    return (null, false);
                
                var avatarUrl = await avatarService.SaveAvatarAsync(new MemoryStream(request.AvatarData), request.Format);

                if (string.IsNullOrEmpty(avatarUrl))
                    return (null, false);

                var account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId);

                if (account == null)
                    return (null, false);

                await avatarService.DeleteAvatarAsync(account.AvatarUrl);

                account.Version++;
                account.AvatarUrl = avatarUrl;

                await dbContext.SaveChangesAsync();

                return (new() { Version = account.Version, Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), AvatarUrl = avatarUrl }, true);
            }
            catch (Exception ex)
            {
                return (null, false);
            }
        }
    }

}
