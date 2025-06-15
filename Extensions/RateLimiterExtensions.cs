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
                #region Account controller

                options.AddFixedWindowLimiter(policyName: "register", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 2;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 0;
                });

                options.AddFixedWindowLimiter(policyName: "username-available", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 30;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 0;
                });

                options.AddFixedWindowLimiter(policyName: "find-contact", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 30;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 0;
                });

                options.AddFixedWindowLimiter(policyName: "update-fullname", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 10;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 0;
                });

                options.AddFixedWindowLimiter(policyName: "update-username", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 6;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 0;
                });

                options.AddFixedWindowLimiter(policyName: "update-bio", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 30;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 0;
                });

                options.AddFixedWindowLimiter(policyName: "update-avatar", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 30;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 0;
                });

                #endregion
            });

            return services;
        }
    }
}
