using ArqanumServer.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ArqanumServer.Services
{
    public interface ISignalRSenderService
    {
        Task<bool> SendAsync(string method, byte[] data, string accountId);
    }
    public class SignalRSenderService(ISignalRConnectionStore signalRConnectionStore, IHubContext<AppHub> hubContext) : ISignalRSenderService
    {
        public async Task<bool> SendAsync(string method ,byte[] data, string accountId)
        {
            try
            {
                var connectionId = await signalRConnectionStore.GetConnectionAsync(accountId);

                if (string.IsNullOrEmpty(connectionId))
                    return false;

                await hubContext.Clients.Client(connectionId).SendAsync(method, data);
                
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
