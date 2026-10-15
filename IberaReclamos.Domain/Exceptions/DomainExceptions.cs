namespace IberaReclamos.Domain.Exceptions
{
    // Excepciones de negocio. Los servicios las lanzan y el ExceptionMiddleware (capa API)
    // las traduce al código HTTP correcto. El mensaje es seguro para mostrarle al usuario.

    // 400: la solicitud no tiene sentido para el negocio (ej: la categoría no existe)
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message) { }
    }

    // 401: credenciales inválidas
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
    }

    // 403: está logueado, pero no puede hacer esto (ej: tocar el reporte de otro ranger)
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }
    }

    // 404: no existe
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    // 409: choca con algo que ya existe (ej: email repetido)
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}
