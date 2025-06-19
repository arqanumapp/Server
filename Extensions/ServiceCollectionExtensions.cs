using ArqanumServer.Crypto;
using ArqanumServer.Data;
using ArqanumServer.Services;
using ArqanumServer.Services.Validators;

namespace ArqanumServer.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddArqanumServices(this IServiceCollection services)
        {
            services.AddTransient<IHCaptchaValidator, HCaptchaValidator>();

            services.AddTransient<IProofOfWorkValidator, ProofOfWorkValidator>();

            services.AddTransient<IShakeGenerator, ShakeGenerator>();

            services.AddTransient<IAccountService, AccountService>();

            services.AddSingleton<IMlDsaKeyVerifier, MlDsaKeyVerifier>();

            services.AddSingleton<ITimestampValidator>(new TimestampValidator(maxSkewSeconds: 30));

            services.AddSingleton<ICloudFileStorage, R2FileStorage>();

            services.AddTransient<IAvatarService, AvatarService>();

            services.AddTransient<IContactService, ContactService>();

            services.AddSingleton<ISignalRConnectionStore, SignalRConnectionStore>();

            services.AddSingleton<ISignalRSenderService, SignalRSenderService>();

            return services;
        }
    }
}
