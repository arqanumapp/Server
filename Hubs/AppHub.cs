using ArqanumServer.Crypto;
using ArqanumServer.Data;
using ArqanumServer.Models.Dtos.Hub;
using ArqanumServer.Services;
using ArqanumServer.Services.Validators;
using MessagePack;
using Microsoft.AspNetCore.SignalR;

namespace ArqanumServer.Hubs
{
    public class AppHub(ISignalRConnectionStore signalRConnectionStore, IMlDsaKeyVerifier mlDsaKeyVerifier, AppDbContext appDbContext, ITimestampValidator timestampValidator, IPendingNotificationService pendingNotificationService) : Hub
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

                byte[] authBytes;
                byte[] signature;

                try
                {
                    authBytes = Convert.FromBase64String(authBase64);
                    signature = Convert.FromBase64String(signatureBase64);
                }
                catch (FormatException ex)
                {
                    Context.Abort();
                    return;
                }

                HubConnectionAuth auth;
                try
                {
                    auth = MessagePackSerializer.Deserialize<HubConnectionAuth>(authBytes);
                }
                catch (Exception ex)
                {
                    Context.Abort();
                    return;
                }

                if (!timestampValidator.IsValid(auth.Timestamp))
                {
                    Context.Abort();
                    return;
                }

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

                if (!await signalRConnectionStore.RemoveAllConnectionsAsync(auth.AccountId))
                {
                    Context.Abort();
                    return;
                }
                if (!await signalRConnectionStore.AddConnectionAsync(auth.AccountId, Context.ConnectionId))
                {
                    Context.Abort();
                    return;
                }
                await base.OnConnectedAsync();

                await pendingNotificationService.SendUpdates(account.Id);
            }
            catch (Exception ex)
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
