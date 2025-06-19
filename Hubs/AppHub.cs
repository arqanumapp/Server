using ArqanumServer.Crypto;
using ArqanumServer.Data;
using ArqanumServer.Models.Dtos.Hub;
using ArqanumServer.Services;
using ArqanumServer.Services.Validators;
using MessagePack;
using Microsoft.AspNetCore.SignalR;

namespace ArqanumServer.Hubs
{
    public class AppHub(ISignalRConnectionStore signalRConnectionStore, IMlDsaKeyVerifier mlDsaKeyVerifier, AppDbContext appDbContext, ITimestampValidator timestampValidator) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var logFile = Path.Combine("logs", "signalr.txt");

            void Log(string message)
            {
                var logLine = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                Directory.CreateDirectory("logs");
                File.AppendAllText(logFile, logLine);
            }

            var httpContext = Context.GetHttpContext();
            var authHeader = httpContext?.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                Log("Missing or invalid Authorization header");
                Context.Abort();
                return;
            }

            var token = authHeader["Bearer ".Length..].Trim();

            try
            {
                var parts = token.Split('|');
                if (parts.Length != 2)
                {
                    Log("Token format invalid (parts.Length != 2)");
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
                    Log($"Base64 decoding failed: {ex}");
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
                    Log($"MessagePack deserialization failed: {ex}");
                    Context.Abort();
                    return;
                }

                if (!timestampValidator.IsValid(auth.Timestamp))
                {
                    Log("Timestamp is not valid");
                    Context.Abort();
                    return;
                }

                var account = await appDbContext.Accounts.FindAsync(auth.AccountId);
                if (account == null)
                {
                    Log($"Account not found: {auth.AccountId}");
                    Context.Abort();
                    return;
                }

                var isValid = await mlDsaKeyVerifier.VerifyAsync(account.SignaturePublicKey, authBytes, signature);
                if (!isValid)
                {
                    Log("Signature verification failed");
                    Context.Abort();
                    return;
                }
                if (!await signalRConnectionStore.RemoveAllConnectionsAsync(auth.AccountId))
                {
                    Log("Failed to remove old connections");
                    Context.Abort();
                    return;
                }
                if (!await signalRConnectionStore.AddConnectionAsync(auth.AccountId, Context.ConnectionId))
                {
                    Context.Abort();
                    return;
                }
                var test = await  signalRConnectionStore.GetConnectionAsync(account.Id);
                Log($"Connection authorized: {auth.AccountId} -> {Context.ConnectionId}");
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                Log($"Unhandled exception: {ex}");
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
