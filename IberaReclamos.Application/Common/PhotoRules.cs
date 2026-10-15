using IberaReclamos.Application.Models;
using IberaReclamos.Domain.Exceptions;

namespace IberaReclamos.Application.Common
{
    // Reglas para las fotos que suben turistas y rangers antes de mandarlas a Cloudflare R2.
    public static class PhotoRules
    {
        // El front de turistas comprime a ~300 KB; los rangers pueden mandar fotos más pesadas desde el celular.
        public const long MaxSizeBytes = 8L * 1024 * 1024;

        private static readonly Dictionary<string, string> Extensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg",
            ["image/jpg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp",
        };

        // Valida tipo, tamaño y que el contenido REALMENTE sea una imagen (mirando los primeros bytes),
        // porque el Content-Type lo manda el cliente y se puede falsificar.
        // Devuelve la extensión y el content-type normalizado.
        public static async Task<(string Extension, string ContentType)> EnsureValidAsync(FileUpload photo, CancellationToken cancellationToken = default)
        {
            if (photo.Length <= 0)
                throw new BadRequestException("La foto está vacía.");

            if (photo.Length > MaxSizeBytes)
                throw new BadRequestException($"La foto puede pesar hasta {MaxSizeBytes / (1024 * 1024)} MB.");

            if (!Extensions.ContainsKey(photo.ContentType))
                throw new BadRequestException("La foto tiene que ser JPG, PNG o WebP.");

            var header = new byte[12];
            var read = await photo.Content.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, cancellationToken);
            if (photo.Content.CanSeek) photo.Content.Position = 0;

            var detected = DetectContentType(header.AsSpan(0, read));
            if (detected is null)
                throw new BadRequestException("El archivo no es una imagen válida.");

            return (Extensions[detected], detected);
        }

        // Clave del objeto en R2, ej: "reports/2026/10/3f6c...e1.jpg"
        public static string BuildObjectKey(string folder, string extension)
        {
            var now = DateTime.UtcNow;
            return $"{folder}/{now:yyyy}/{now:MM}/{Guid.NewGuid():N}{extension}";
        }

        private static string? DetectContentType(ReadOnlySpan<byte> h)
        {
            if (h.Length >= 3 && h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF) return "image/jpeg";
            if (h.Length >= 8 && h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47) return "image/png";
            if (h.Length >= 12 && h[0] == 0x52 && h[1] == 0x49 && h[2] == 0x46 && h[3] == 0x46
                && h[8] == 0x57 && h[9] == 0x45 && h[10] == 0x42 && h[11] == 0x50) return "image/webp";
            return null;
        }
    }
}
