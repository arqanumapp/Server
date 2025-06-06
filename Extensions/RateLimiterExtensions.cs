using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace ArqanumServer.Extensions
{
    public static class RateLimiterExtensions
    {
        public static IServiceCollection AddCustomRateLimiters(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter(policyName: "register", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 1;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 0;
                });

                options.AddFixedWindowLimiter(policyName: "username-available", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 60;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 0;
                });
            });

            return services;
        }
    }
}
