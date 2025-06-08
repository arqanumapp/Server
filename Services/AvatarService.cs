using ArqanumServer.Data;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ArqanumServer.Services
{
    public interface IAvatarService
    {
        Task<string> SaveAvatarAsync(Stream stream, string format);

        Task DeleteAvatarAsync(string path);

        Task<Stream> GenerateAvatarStream(char initial);
    }

    public class AvatarService(ICloudFileStorage cloud) : IAvatarService
    {
        public async Task<string> SaveAvatarAsync(Stream stream, string format)
        {
            string contentType = format.ToLowerInvariant() switch
            {
                "png" => "image/png",
                "jpg" or "jpeg" => "image/jpeg",
                _ => throw new ArgumentException($"Unsupported format: {format}")
            };
            var path = $"avatars/{Guid.NewGuid()}.{format}";
            return await cloud.PublicUploadAsync(path, stream, contentType);
        }

        public Task DeleteAvatarAsync(string path)
        {   
            return cloud.DeleteAsync(path);
        }

        public async Task<Stream> GenerateAvatarStream(char initial)
        {
            Color[] colors =
            [
                Color.ParseHex("e74c3c"), Color.ParseHex("9b59b6"), Color.ParseHex("3498db"),
                Color.ParseHex("e67e22"), Color.ParseHex("1abc9c"), Color.ParseHex("2ecc71"),
                Color.ParseHex("f39c12"), Color.ParseHex("d35400"), Color.ParseHex("8e44ad"),
                Color.ParseHex("27ae60"),
            ];

            var random = new Random();
            var bgColor = colors[random.Next(colors.Length)];
            var font = SystemFonts.CreateFont("Arial", 500, FontStyle.Bold);
            var text = initial.ToString().ToUpper();

            var options = new RichTextOptions(font)
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Origin = new SixLabors.ImageSharp.PointF(500, 500)
            };

            using var image = new Image<Rgba32>(1000, 1000);
            image.Mutate(ctx =>
            {
                ctx.Fill(bgColor);
                ctx.DrawText(options, text, Color.White);
            });

            var ms = new MemoryStream();
            await image.SaveAsPngAsync(ms);
            ms.Position = 0;
            return ms;
        }

    }
}
