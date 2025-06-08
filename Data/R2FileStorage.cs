using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace ArqanumServer.Data
{
    public interface ICloudFileStorage
    {
        Task<string> PrivateUploadAsync(string path, Stream data, string contentType, CancellationToken cancellationToken = default);

        Task<string> PublicUploadAsync(string path, Stream data, string contentType, CancellationToken cancellationToken = default);

        Task DeleteAsync(string path, CancellationToken cancellationToken = default);

        string GetPrivateUrl(string path, DateTime? expires = null);

        string GetPublicUrl(string path);
    }

    public class R2FileStorage(IAmazonS3 s3Client, IOptions<R2StorageSettings> options) : ICloudFileStorage
    {
        private readonly R2StorageSettings _settings = options.Value;

        public async Task<string> PrivateUploadAsync(string path, Stream data, string contentType, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new PutObjectRequest
                {
                    BucketName = _settings.BucketName,
                    DisablePayloadSigning = true,
                    DisableDefaultChecksumValidation = true,
                    Key = path,
                    InputStream = data,
                    ContentType = "application/octet-stream",
                    AutoCloseStream = true
                };

                var response = await s3Client.PutObjectAsync(request, cancellationToken);

                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception($"Upload failed with status: {response.HttpStatusCode}, RequestId: {response.ResponseMetadata.RequestId}");

                return GetPrivateUrl(path);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upload file to R2: {ex.Message}", ex);
            }
        }
        public async Task<string> PublicUploadAsync(string path, Stream data, string contentType, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new PutObjectRequest
                {
                    BucketName = _settings.BucketName,
                    DisablePayloadSigning = true,
                    DisableDefaultChecksumValidation = true,
                    Key = path,
                    InputStream = data,
                    ContentType = "application/octet-stream",
                    CannedACL = S3CannedACL.PublicRead,
                    AutoCloseStream = true
                };

                var response = await s3Client.PutObjectAsync(request, cancellationToken);

                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception($"Upload failed with status: {response.HttpStatusCode}, RequestId: {response.ResponseMetadata.RequestId}");

                return GetPublicUrl(path);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upload file to R2: {ex.Message}", ex);
            }
        }

        public async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = path
            };

            await s3Client.DeleteObjectAsync(request, cancellationToken);
        }
        public string GetPublicUrl(string path)
        {
            return $"{_settings.PublicUrl}/{path}";
        }
        public string GetPrivateUrl(string path, DateTime? expires = null)
        {
            var presign = new GetPreSignedUrlRequest
            {
                BucketName = _settings.BucketName,
                Key = path,
                Verb = HttpVerb.GET,
                Expires = expires ?? DateTime.UtcNow.AddDays(7),
            };

            var presignedUrl = s3Client.GetPreSignedURL(presign);

            return presignedUrl;
        }
    }
}
