using ArqanumServer.Crypto;
using ArqanumServer.Data;
using ArqanumServer.Models.Dtos.Account;
using ArqanumServer.Models.Dtos.Contact;
using ArqanumServer.Services.Validators;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace ArqanumServer.Services
{
    public interface IContactService
    {
        Task<GetContactResponceDto?> FindContactAsync(byte[] requestSignature, byte[] rawData);
    }

    public class ContactService(AppDbContext appDbContext, ITimestampValidator timestampValidator, IMlDsaKeyVerifier mlDsaKeyVerifier) : IContactService
    {
        public async Task<GetContactResponceDto?> FindContactAsync(byte[] requestSignature, byte[] rawData)
        {
            var request = MessagePackSerializer.Deserialize<GetContactRequestDto>(rawData);

            if (!timestampValidator.IsValid(request.Timestamp))
                throw new ArgumentException("Invalid timestamp.", nameof(request.Timestamp));


            var accountPublicKeyBytes = await appDbContext.Accounts
                .AsNoTracking()
                .Where(a => a.Id == request.AccountId)
                .Select(a => a.SignaturePublicKey)
                .FirstOrDefaultAsync() ?? throw new ArgumentException();

            if (!await mlDsaKeyVerifier.VerifyAsync(accountPublicKeyBytes, rawData, requestSignature))
                throw new UnauthorizedAccessException("Invalid signature.");


            var dto = await appDbContext.Accounts
                .AsNoTracking()
                .Where(a => a.Id == request.ContactIdentifier || a.Username == request.ContactIdentifier)
                .Select(a => new GetContactResponceDto
                {
                    ContactId = a.Id,
                    Username = a.Username,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    Bio = a.Bio,
                    AvatarUrl = a.AvatarUrl
                })
                .FirstOrDefaultAsync();

            return dto;
        }
    }

}
