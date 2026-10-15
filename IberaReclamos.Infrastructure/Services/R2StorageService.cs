using Amazon.S3;
using Amazon.S3.Model;
using IberaReclamos.Application.Common.Options;
using IberaReclamos.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IberaReclamos.Infrastructure.Services
{
    // Sube las fotos a Cloudflare R2 usando el SDK de Amazon S3 (R2 habla el mismo "idioma" que S3).
    // La DB solo guarda la ObjectKey y la URL pública que devuelve este servicio.
    public sealed class R2StorageService : IStorageService, IDisposable
    {
        private readonly IAmazonS3 _s3;
        private readonly R2Options _options;
        private readonly ILogger<R2StorageService> _logger;

        public R2StorageService(IOptions<R2Options> options, ILogger<R2StorageService> logger)
        {
            _options = options.Value;
            _logger = logger;

            var config = new AmazonS3Config
            {
                ServiceURL = _options.ServiceUrl,   // https://<account-id>.r2.cloudflarestorage.com
                ForcePathStyle = true,              // R2 usa rutas tipo /bucket/key
                AuthenticationRegion = "auto"       // R2 no tiene regiones: siempre "auto"
            };

            _s3 = new AmazonS3Client(_options.AccessKeyId, _options.SecretAccessKey, config);
        }

        public string BuildPublicUrl(string objectKey)
        {
            var baseUrl = _options.PublicBaseUrl.TrimEnd('/');
            return $"{baseUrl}/{objectKey.TrimStart('/')}";
        }

        public async Task<StorageUploadResult> UploadAsync(
            Stream content,
            string objectKey,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            if (content.CanSeek) content.Position = 0;

            var request = new PutObjectRequest
            {
                BucketName = _options.BucketName,
                Key = objectKey,
                InputStream = content,
                ContentType = contentType,
                // R2 no soporta "streaming signatures" de S3: hay que desactivar el payload signing
                DisablePayloadSigning = true,
                Headers =
                {
                    // Cada foto tiene un nombre único (Guid), así que el navegador/CDN puede cachearla para siempre
                    CacheControl = "public, max-age=31536000, immutable"
                }
            };

            await _s3.PutObjectAsync(request, cancellationToken);

            var size = content.CanSeek ? content.Length : 0;
            _logger.LogInformation("Uploaded object to R2. Bucket={Bucket} Key={Key} Size={Size}", _options.BucketName, objectKey, size);

            return new StorageUploadResult(objectKey, BuildPublicUrl(objectKey), contentType, size);
        }

        public async Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default)
        {
            try
            {
                await _s3.DeleteObjectAsync(_options.BucketName, objectKey, cancellationToken);
                _logger.LogInformation("Deleted R2 object. Bucket={Bucket} Key={Key}", _options.BucketName, objectKey);
            }
            catch (AmazonS3Exception ex)
            {
                // Borrar una foto vieja no debe romper el request del usuario: solo lo registramos
                _logger.LogWarning(ex, "Could not delete R2 object {Key}", objectKey);
            }
        }

        public void Dispose() => _s3.Dispose();
    }
}
