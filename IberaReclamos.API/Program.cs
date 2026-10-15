using System.Text.Json.Serialization;
using FluentValidation;
using IberaReclamos.API.Filters;
using IberaReclamos.Application.Interfaces.Common;
using IberaReclamos.Application.Interfaces.Services;
using IberaReclamos.Application.Services;
using IberaReclamos.Application.Validators;
using IberaReclamos.Domain.Interfaces;
using IberaReclamos.Infrastructure;
using IberaReclamos.Infrastructure.Persistence;
using IberaReclamos.Infrastructure.Persistence.Repositories;
using IberaReclamos.Infrastructure.Persistence.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Fail fast: si falta la cadena de conexión la API no arranca (nunca la guardes en appsettings.json).
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(defaultConnection))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection must be configured (User Secrets or env var ConnectionStrings__DefaultConnection). " +
        "See appsettings.example.json.");
}

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
// VALIDACIONES (FluentValidation)
// =========================================================================
// Registra TODOS los AbstractValidator<T> que haya en el proyecto Application (Validators/).
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();

#endregion


#region CONEXIÓN A BASE DE DATOS (PostgreSQL)
// =========================================================================
// 2. CONEXIÓN A BASE DE DATOS (PostgreSQL)
// =========================================================================
builder.Services.AddDbContext<IberaReclamosDbContext>(options =>
    options.UseNpgsql(defaultConnection));

#endregion


#region INYECCIÓN DE DEPENDENCIAS - REPOSITORIOS Y UNIT OF WORK
// =========================================================================
// INYECCIÓN DE DEPENDENCIAS - REPOSITORIOS Y UNIT OF WORK
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
// INYECCIÓN DE DEPENDENCIAS - SERVICIOS (Lógica de Negocio)
// =========================================================================
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IReportCategoryService, ReportCategoryService>();
builder.Services.AddScoped<IRangerTaskService, RangerTaskService>();

#endregion


#region INFRAESTRUCTURA / SEGURIDAD
// =========================================================================
// INFRAESTRUCTURA / SEGURIDAD
// =========================================================================
// Singleton: BCrypt no guarda estado, una sola instancia sirve para toda la app.
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

#endregion


#region CONSTRUCCIÓN DE LA APP
// =========================================================================
// 3. CONSTRUCCIÓN DE LA APP
// =========================================================================
var app = builder.Build();

#endregion


#region PIPELINE DE LA APP
// =========================================================================
// 4. PIPELINE DE LA APLICACIÓN (ORDEN CRÍTICO)
// =========================================================================
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
#endregion
