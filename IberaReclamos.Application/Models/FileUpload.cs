namespace IberaReclamos.Application.Models
{
    // Archivo subido (foto) sin depender de ASP.NET (IFormFile vive en la capa API).
    // El controller convierte IFormFile -> FileUpload y se lo pasa al servicio.
    public sealed record FileUpload(Stream Content, string FileName, string ContentType, long Length);
}
