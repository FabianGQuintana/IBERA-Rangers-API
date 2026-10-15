using System.Globalization;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using FluentValidation;
using IberaReclamos.API.Filters;
using IberaReclamos.API.Middleware;
using IberaReclamos.Application.Common.Options;
using IberaReclamos.Application.Models;
using IberaReclamos.Domain.Constants;
using IberaReclamos.Infrastructure.Persistence.Seed;
using Microsoft.OpenApi;
using IberaReclamos.Application.Interfaces.Common;
using IberaReclamos.Application.Interfaces.Services;
using IberaReclamos.Application.Services;
using IberaReclamos.Application.Validators;
using IberaReclamos.Domain.Interfaces;
using IberaReclamos.Infrastructure;
using IberaReclamos.Infrastructure.Persistence;
using IberaReclamos.Infrastructure.Persistence.Repositories;
using IberaReclamos.Infrastructure.Persistence.Security;
using IberaReclamos.Infrastructure.Services;
using IberaReclamos.Infrastructure.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

// Cultura invariante: los formularios (multipart) traen números con punto decimal ("-28.5").
// Con la cultura del servidor en español, "-28.5" se leería mal (la coma es el separador decimal).
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// Fail fast: si faltan secretos la API no arranca (nunca los guardes en appsettings.json).
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(defaultConnection))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection must be configured (User Secrets or env var ConnectionStrings__DefaultConnection). " +
        "See appsettings.example.json.");
}

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwtOptions.Key) || jwtOptions.Key.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key must be configured (User Secrets or env var Jwt__Key) with at least 32 characters. " +
        "See appsettings.example.json.");
}

// Tope del tamaño de los requests (fotos de hasta 8 MB + datos del formulario).
const long MaxUploadBytes = 10L * 1024 * 1024;
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = MaxUploadBytes);
builder.Services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = MaxUploadBytes);

#region CONFIGURACIÓN DE CONTROLADORES
// =========================================================================
// 1. CONFIGURACIÓN DE CONTROLADORES
// =========================================================================
builder.Services.AddControllers(options =>
    {
        // Valida automáticamente cada DTO con su validador de FluentValidation antes de entrar al controller.
        options.Filters.Add<ValidationFilter>();
    })
    .AddJsonOptions(options =>
    {
        // Acepta y devuelve enums como texto ("Pending") en vez de números.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Desactivamos el 400 automático de [ApiController]: los errores de validación los arma nuestro ValidationFilter
// con el mismo formato que el resto de la API.
builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

#endregion


#region SWAGGER (Documentación de la API)
// =========================================================================
// SWAGGER (Documentación de la API) -> /swagger en Development
// =========================================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IberaReclamos API",
        Version = "v1",
        Description = "Reclamos de turistas y reportes ambientales de los guardaparques del Iberá."
    });

    // Botón "Authorize" de Swagger: pegás el token del login y Swagger lo manda en cada request
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegá solo el token (sin la palabra Bearer)."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });

    // Usa los comentarios /// de los controllers como descripción de cada endpoint
    var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
});

#endregion


#region VALIDACIONES (FluentValidation)
// =========================================================================
// 2. VALIDACIONES (FluentValidation)
// =========================================================================
// Registra TODOS los AbstractValidator<T> que haya en el proyecto Application (Validators/).
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();

#endregion


#region CONEXIÓN A BASE DE DATOS (PostgreSQL)
// =========================================================================
// 3. CONEXIÓN A BASE DE DATOS (PostgreSQL)
// =========================================================================
builder.Services.AddDbContext<IberaReclamosDbContext>(options =>
    options.UseNpgsql(defaultConnection));

#endregion


#region INYECCIÓN DE DEPENDENCIAS - REPOSITORIOS Y UNIT OF WORK
// =========================================================================
// 4. INYECCIÓN DE DEPENDENCIAS - REPOSITORIOS Y UNIT OF WORK
// =========================================================================
// Scoped = una instancia por request HTTP, igual que el DbContext que comparten.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IRangerTaskRepository, RangerTaskRepository>();
builder.Services.AddScoped<ITouristRepository, TouristRepository>();

#endregion


#region INYECCIÓN DE DEPENDENCIAS - SERVICIOS (Lógica de Negocio)
// =========================================================================
// 5. INYECCIÓN DE DEPENDENCIAS - SERVICIOS (Lógica de Negocio)
// =========================================================================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IReportCategoryService, ReportCategoryService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IRangerTaskService, RangerTaskService>();
builder.Services.AddScoped<ITouristService, TouristService>();

#endregion


#region INFRAESTRUCTURA / SEGURIDAD
// =========================================================================
// 6. INFRAESTRUCTURA / SEGURIDAD
// =========================================================================
// Singleton: BCrypt no guarda estado, una sola instancia sirve para toda la app.
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

// JWT: JwtUtils genera y valida los tokens
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddSingleton<JwtUtils>();
builder.Services.AddSingleton<ITokenService>(sp => sp.GetRequiredService<JwtUtils>());

// Fotos: Cloudflare R2 si está configurado; si no, disco local (wwwroot/uploads) para desarrollo
builder.Services.Configure<R2Options>(builder.Configuration.GetSection(R2Options.SectionName));

var webRootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
Directory.CreateDirectory(webRootPath);

builder.Services.AddSingleton<IStorageService>(sp =>
{
    var r2 = sp.GetRequiredService<IOptions<R2Options>>().Value;
    if (r2.IsConfigured)
        return ActivatorUtilities.CreateInstance<R2StorageService>(sp);

    if (builder.Environment.IsProduction())
        throw new InvalidOperationException("R2 must be configured in Production (see appsettings.example.json).");

    return ActivatorUtilities.CreateInstance<LocalDiskStorageService>(sp, webRootPath);
});

#endregion


#region AUTENTICACIÓN JWT
// =========================================================================
// 7. AUTENTICACIÓN JWT
// =========================================================================
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Dejamos los claims con el nombre con que los emitimos ("id", "email"...) sin traducirlos
        options.MapInboundClaims = false;
        options.TokenValidationParameters = JwtUtils.BuildValidationParameters(jwtOptions);
    });

builder.Services.AddAuthorization();

#endregion


#region RATE LIMITING
// =========================================================================
// RATE LIMITING (frena bots y fuerza bruta en los endpoints públicos)
// =========================================================================
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(new ApiErrorResponse
        {
            Code = ErrorCodes.TooManyRequests,
            Message = "Hiciste demasiados intentos. Esperá un minuto y probá de nuevo.",
            TraceId = context.HttpContext.TraceIdentifier
        }, cancellationToken);
    };

    // Login: 10 intentos por minuto por IP
    options.AddPolicy("AuthLoginPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));

    // Formulario público de turistas: 20 envíos por minuto por IP
    // (generoso: la cola offline del front puede mandar varias valoraciones juntas al recuperar señal)
    options.AddPolicy("PublicFeedbackPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});

#endregion


#region CORS (Acceso desde los Frontends)
// =========================================================================
// CORS (Acceso desde los Frontends: web de turistas y app de rangers)
// =========================================================================
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:3000", "http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("IberaFrontendPolicy", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

#endregion


#region CONSTRUCCIÓN DE LA APP
// =========================================================================
// 8. CONSTRUCCIÓN DE LA APP
// =========================================================================
var app = builder.Build();

#endregion


#region PIPELINE DE LA APP
// =========================================================================
// 9. PIPELINE DE LA APLICACIÓN (ORDEN CRÍTICO)
// =========================================================================
// 0. Migraciones (si Database:MigrateOnStartup = true) y primer admin (si SeedAdmin está configurado)
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(
        scope.ServiceProvider.GetRequiredService<IberaReclamosDbContext>(),
        scope.ServiceProvider.GetRequiredService<IPasswordHasher>(),
        app.Configuration,
        app.Logger);
}

// 1. Red de seguridad: va PRIMERO para atrapar las excepciones de todo lo que viene después
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.DocumentTitle = "IberaReclamos API");
}

app.UseHttpsRedirection();

// 2. Sirve wwwroot (fotos guardadas en disco cuando R2 no está configurado).
// FileProvider explícito: si wwwroot no existía al crear el builder (primer arranque), el UseStaticFiles()
// por defecto no ve la carpeta que creamos arriba y las fotos devuelven 404.
app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(webRootPath) });

// 3. CORS: dejamos entrar las peticiones de los frontends
app.UseCors("IberaFrontendPolicy");

// 4. Rate limiting
app.UseRateLimiter();

// 5. AUTHENTICATION: ¿Quién sos? (Lee el Token)
app.UseAuthentication();

// 6. AUTHORIZATION: ¿Tenés permiso para esto? (Mira los Roles)
app.UseAuthorization();

// 7. ROUTING: mandamos la petición al controlador
app.MapControllers();

app.Run();
#endregion
