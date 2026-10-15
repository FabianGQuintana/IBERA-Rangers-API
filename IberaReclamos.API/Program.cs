using System.Text.Json.Serialization;
using FluentValidation;
using IberaReclamos.API.Filters;
using IberaReclamos.Application.Common.Options;
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
using Microsoft.Extensions.Options;

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

builder.Services.AddOpenApi();

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
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Sirve wwwroot (fotos guardadas en disco cuando R2 no está configurado)
app.UseStaticFiles();

// AUTHENTICATION: ¿Quién sos? (Lee el Token)
app.UseAuthentication();

// AUTHORIZATION: ¿Tenés permiso para esto? (Mira los Roles)
app.UseAuthorization();

app.MapControllers();

app.Run();
#endregion
