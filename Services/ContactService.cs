using ArqanumServer.Crypto;
using ArqanumServer.Data;
using ArqanumServer.Models.Dtos.Contact;
using ArqanumServer.Models.Dtos.Hub.Contact;
using ArqanumServer.Models.Entities;
using ArqanumServer.Services.Validators;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace ArqanumServer.Services
{
    public interface IContactService
    {
        Task<GetContactResponceDto?> FindContactAsync(byte[] requestSignature, byte[] rawData);

        Task<bool> AddContactRequest(byte[] requestSignature, byte[] rawData);
    }

    public class ContactService(AppDbContext appDbContext, ITimestampValidator timestampValidator, IMlDsaKeyVerifier mlDsaKeyVerifier, ISignalRSenderService signalRSenderService) : IContactService
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
                    Version = a.Version,
                    AvatarUrl = a.AvatarUrl
                })
                .FirstOrDefaultAsync();

            dto.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            return dto;
        }

        public async Task<bool> AddContactRequest(byte[] requestSignature, byte[] rawData)
        {
            try
            {
                var request = MessagePackSerializer.Deserialize<AddContactRequestDto>(rawData);

                if (!timestampValidator.IsValid(request.Timestamp))
                    return false;

                var payload = MessagePackSerializer.Deserialize<ContactPayload>(request.Payload);

                var accountPublicKeyBytes = await appDbContext.Accounts
                    .AsNoTracking()
                    .Where(a => a.Id == payload.SenderId)
                    .Select(a => a.SignaturePublicKey)
                    .FirstOrDefaultAsync() ?? throw new ArgumentException();

                if (!await mlDsaKeyVerifier.VerifyAsync(accountPublicKeyBytes, rawData, requestSignature))
                    return false;

                var responcePyload = new ContactPayload
                {
                    SenderId = payload.SenderId,
                    SignaturePublicKey = accountPublicKeyBytes,
                    ContactPublicKey = request.PayloadSignature
                };

                var responcePayload = new BaseContactHubMessage();

                responcePayload.Payload = request.Payload;
                responcePayload.PayloadSignature = request.PayloadSignature;
                responcePayload.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                responcePayload.MessageType = ContactHubMessageType.NewContactRequest;

                var responcePayloadBytes = MessagePackSerializer.Serialize(responcePayload);   

                if (await signalRSenderService.SendAsync("Contact", responcePayloadBytes, request.RecipientId))
                {
                    return true;
                }
                else
                {
                    var requestData = new Request();
                    requestData.RecipientId = request.RecipientId;
                    requestData.Payload = request.Payload;
                    requestData.PayloadSignature = request.PayloadSignature;
                    appDbContext.Requests.Add(requestData);
                    await appDbContext.SaveChangesAsync();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }

}
