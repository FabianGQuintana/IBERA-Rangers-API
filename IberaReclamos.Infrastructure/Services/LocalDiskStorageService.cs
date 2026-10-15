using IberaReclamos.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace IberaReclamos.Infrastructure.Services
{
    // Plan B para desarrollo: si R2 no está configurado, guarda las fotos en wwwroot/uploads
    // y la API las sirve como archivos estáticos (/uploads/...). No usar en producción.
    public sealed class LocalDiskStorageService : IStorageService
    {
        private const string UploadsFolder = "uploads";

        private readonly string _webRootPath;
        private readonly ILogger<LocalDiskStorageService> _logger;

        public LocalDiskStorageService(string webRootPath, ILogger<LocalDiskStorageService> logger)
        {
            _webRootPath = webRootPath;
            _logger = logger;
        }

        public string BuildPublicUrl(string objectKey) => $"/{UploadsFolder}/{objectKey.TrimStart('/')}";

        public async Task<StorageUploadResult> UploadAsync(Stream content, string objectKey, string contentType, CancellationToken cancellationToken = default)
        {
            var fullPath = GetSafePath(objectKey);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            if (content.CanSeek) content.Position = 0;
            await using (var file = File.Create(fullPath))
            {
                await content.CopyToAsync(file, cancellationToken);
            }

            var size = new FileInfo(fullPath).Length;
            _logger.LogInformation("Saved object to local disk. Key={Key} Size={Size}", objectKey, size);

            return new StorageUploadResult(objectKey, BuildPublicUrl(objectKey), contentType, size);
        }

        public Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default)
        {
            var fullPath = GetSafePath(objectKey);
            if (File.Exists(fullPath)) File.Delete(fullPath);
            return Task.CompletedTask;
        }

        // Evita "path traversal" (una key como ../../appsettings.json no puede salir de wwwroot/uploads)
        private string GetSafePath(string objectKey)
        {
            var root = Path.GetFullPath(Path.Combine(_webRootPath, UploadsFolder));
            var fullPath = Path.GetFullPath(Path.Combine(root, objectKey.Replace('/', Path.DirectorySeparatorChar)));

            if (!fullPath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Invalid object key.");

            return fullPath;
        }
    }
}
