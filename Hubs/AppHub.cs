using ArqanumServer.Crypto;
using ArqanumServer.Data;
using ArqanumServer.Models.Dtos.Hub;
using ArqanumServer.Services;
using MessagePack;
using Microsoft.AspNetCore.SignalR;

namespace ArqanumServer.Hubs
{
    public class AppHub(ISignalRConnectionStore signalRConnectionStore, IMlDsaKeyVerifier mlDsaKeyVerifier, AppDbContext appDbContext) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var authHeader = httpContext?.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                Context.Abort();
                return;
            }

            var token = authHeader["Bearer ".Length..].Trim();

            try
            {
                var parts = token.Split('|');
                if (parts.Length != 2)
                {
                    Context.Abort();
                    return;
                }

                var authBase64 = parts[0];
                var signatureBase64 = parts[1];

                var authBytes = Convert.FromBase64String(authBase64);
                var signature = Convert.FromBase64String(signatureBase64);

                var auth = MessagePackSerializer.Deserialize<HubConnectionAuth>(authBytes);

                var account = await appDbContext.Accounts.FindAsync(auth.AccountId);
                if (account == null)
                {
                    Context.Abort();
                    return;
                }

                var isValid = await mlDsaKeyVerifier.VerifyAsync(account.SignaturePublicKey, authBytes, signature);
                if (!isValid)
                {
                    Context.Abort();
                    return;
                }

                await signalRConnectionStore.AddConnectionAsync(auth.AccountId, Context.ConnectionId);
                await base.OnConnectedAsync();
            }
            catch
            {
                Context.Abort();
            }
        }


        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                await signalRConnectionStore.RemoveConnectionAsync(Context.ConnectionId);

            }
            catch { }
            finally
            {
                await base.OnDisconnectedAsync(exception);
            }
        }

    }
}
