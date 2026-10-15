namespace IberaReclamos.Application.Interfaces.Services
{
    public sealed record StorageUploadResult(
        string ObjectKey,
        string PublicUrl,
        string ContentType,
        long SizeBytes);

    // Almacenamiento de archivos (fotos). La implementación con Cloudflare R2 vive en Infrastructure.
    // Los servicios solo conocen este contrato: guardan en la DB la ObjectKey y la PublicUrl, nunca el archivo.
    public interface IStorageService
    {
        Task<StorageUploadResult> UploadAsync(
            Stream content,
            string objectKey,
            string contentType,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default);

        string BuildPublicUrl(string objectKey);
    }
}
