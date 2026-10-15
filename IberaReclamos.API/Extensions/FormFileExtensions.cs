using IberaReclamos.Application.Models;

namespace IberaReclamos.API.Extensions
{
    public static class FormFileExtensions
    {
        // IFormFile es de ASP.NET; la capa Application recibe un FileUpload "neutro".
        public static FileUpload? ToFileUpload(this IFormFile? file) =>
            file is null || file.Length == 0
                ? null
                : new FileUpload(file.OpenReadStream(), file.FileName, file.ContentType, file.Length);
    }
}
