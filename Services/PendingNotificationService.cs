using ArqanumServer.Data;
using ArqanumServer.Models.Dtos.Hub.Contact;
using ArqanumServer.Models.Entities;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace ArqanumServer.Services
{
    public interface IPendingNotificationService
    {
        Task SendUpdates(string accountId);
    }
    public class PendingNotificationService(AppDbContext appDbContext, ISignalRSenderService signalRSenderService) : IPendingNotificationService
    {
        public async Task SendUpdates(string accountId)
        {
            var requests = await appDbContext.Requests
                .Where(r => r.RecipientId == accountId)
                .ToListAsync();

            foreach (var request in requests)
            {
                await signalRSenderService.SendAsync(request.Method.ToString(), request.Payload, accountId);
            }

        }
    }
}
