using ArqanumServer.Crypto;
using ArqanumServer.Services;
using ArqanumServer.Services.Validators;

namespace ArqanumServer.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddArqanumServices(this IServiceCollection services)
        {
            services.AddScoped<IHCaptchaValidator, HCaptchaValidator>();

            services.AddScoped<IProofOfWorkValidator, ProofOfWorkValidator>();

            services.AddScoped<IShakeGenerator, ShakeGenerator>();

            services.AddScoped<IAccountService, AccountService>();

            services.AddSingleton<IMlDsaKeyVerifier, MlDsaKeyVerifier>();

            services.AddSingleton<ITimestampValidator>(new TimestampValidator(maxSkewSeconds: 30));

            return services;
        }
    }
}
