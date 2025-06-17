using Amazon.Runtime;
using Amazon.S3;
using ArqanumServer.Data;
using ArqanumServer.Extensions;
using ArqanumServer.Hubs;
using ArqanumServer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection")));

builder.Services.Configure<R2StorageSettings>(builder.Configuration.GetSection("R2Storage"));

builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<R2StorageSettings>>().Value;

    var credentials = new BasicAWSCredentials(settings.AccessKey, settings.SecretKey);
    var config = new AmazonS3Config
    {
        ServiceURL = settings.Endpoint,
        AuthenticationRegion = "auto",
        ForcePathStyle = true,
        AuthenticationServiceName = "s3"
    };

    return new AmazonS3Client(credentials, config);
});

builder.Services.AddControllersWithViews();

builder.Services.AddMvc();

builder.Services.AddCustomRateLimiters();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("RedisConnection")));

builder.Services.AddSignalR().AddMessagePackProtocol();

builder.Services.AddArqanumServices();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapHub<AppHub>("/hub/app");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();

