# IberaReclamos API (IBERA-Rangers-API)

Backend del sistema de reclamos y reportes ambientales del **Gran Parque Iberá** (Desafío Iberá · Hackathon).

Atiende dos públicos:

| Módulo | Quién lo usa | Qué hace |
| --- | --- | --- |
| **Público** | Turistas, desde [IberAPP](https://github.com/KaibaBenja/IberAPP) (web de turistas) | Valorar un portal del parque, dejar sugerencias o quejas con foto opcional y email. Sin cuenta. |
| **Privado** | Guardaparques (Rangers) y administradores, desde [IBERA-Rangers-Client](https://github.com/FabianGQuintana/IBERA-Rangers-Client) | Reportar incidentes ambientales con foto y GPS **sin señal** y sincronizarlos todos juntos (bulk insert) al recuperar conexión. Recibir tareas asignadas. |

---

## Índice

- [Stack](#stack)
- [Arquitectura](#arquitectura)
- [Modelo de datos](#modelo-de-datos)
- [Endpoints](#endpoints)
- [Cómo funciona la sincronización offline](#cómo-funciona-la-sincronización-offline)
- [Fotos: Cloudflare R2](#fotos-cloudflare-r2)
- [Configuración y secretos](#configuración-y-secretos)
- [Correrlo en tu máquina](#correrlo-en-tu-máquina)
- [Cómo probarlo paso a paso](#cómo-probarlo-paso-a-paso)
- [Deploy](#deploy)
- [Forma de trabajo](#forma-de-trabajo)

---

## Stack

- **.NET 10** · ASP.NET Core Web API con controllers
- **Entity Framework Core 10** + **PostgreSQL** (Npgsql)
- **FluentValidation** para validar DTOs
- **BCrypt.Net-Next** para contraseñas
- **JWT** (HMAC-SHA256) para autenticación por roles
- **AWS SDK S3 → Cloudflare R2** para guardar fotos
- **Swagger** (Swashbuckle) para documentar y probar la API

## Arquitectura

Clean Architecture en 4 proyectos. Las dependencias apuntan siempre hacia adentro (Domain no conoce a nadie).

```
IberaReclamos.API             → Controllers, ExceptionMiddleware, ValidationFilter, Program.cs (DI y pipeline)
        │
        ▼
IberaReclamos.Infrastructure  → DbContext, Fluent API, migraciones, repositorios, UnitOfWork,
        │                       JwtUtils, BCrypt, R2StorageService, DbSeeder
        ▼
IberaReclamos.Application     → DTOs, validadores, mappers manuales, servicios (lógica de negocio),
        │                       interfaces de servicios (IStorageService, ITokenService, IPasswordHasher)
        ▼
IberaReclamos.Domain          → Entidades puras, enums, excepciones de negocio,
                                interfaces de repositorio (IGenericRepository<T>, IUnitOfWork), constantes
```

Flujo de un request:

```
HTTP → ExceptionMiddleware → Auth JWT → [Authorize(Roles)] → ValidationFilter (FluentValidation)
     → Controller → Service → UnitOfWork / Repositories → DbContext → PostgreSQL
                          └→ IStorageService → Cloudflare R2 (fotos)
```

Errores: los servicios lanzan excepciones de negocio (`NotFoundException`, `ConflictException`, …) y el `ExceptionMiddleware` las traduce a una respuesta estándar:

```json
{ "code": "VALIDATION_FAILED", "message": "El DNI ingresado no es válido.", "traceId": "…", "errors": { "dni": ["…"] } }
```

## Modelo de datos

| Tabla | Para qué | Claves |
| --- | --- | --- |
| `Roles` | Admin, Ranger, Helper (sembrados con ids fijos) | `RolName` único |
| `Users` | Personal interno | `Email` y `Dni` únicos, DNI de 7-8 dígitos (check), FK `IdRol` |
| `ReportCategories` | Lixiviados, Plásticos y residuos, Incendio, Caza furtiva, Fauna herida, Infraestructura, Otro (sembradas con ids fijos) | `Name` único |
| `Reports` | Incidente: GPS (lat/long), fecha de captura y de sincronización, estado, foto | `ClientId` único (idempotencia offline), FK a categoría y ranger |
| `Tasks` | Tarea asignada a un ranger, opcionalmente ligada a un reporte | FK `AssignedById`, `AssignedToId`, `ReportId` |
| `Tourists` | Valoración/queja del turista: portal, tipo, estrellas 1-5, mensaje, email, foto | `ClientCode` único |

Todas heredan de `BaseEntity` (Guid, auditoría y borrado lógico). Enums guardados como texto, borrado restrictivo en todas las FK y filtro global de `IsDeleted` en reportes, tareas y valoraciones.

## Endpoints

Documentación interactiva en **`/swagger`** (solo en Development).

| Método | Ruta | Quién | Descripción |
| --- | --- | --- | --- |
| POST | `/api/auth/login` | Público | Devuelve `{ token, expiresAt, user }` |
| GET | `/api/auth/me` | Logueado | Datos del usuario del token |
| PUT | `/api/auth/me/password` | Logueado | Cambiar la propia contraseña |
| GET · POST | `/api/users` | Admin | Listar / crear usuarios |
| GET · PUT | `/api/users/{id}` | Admin | Ver / editar usuario |
| PATCH | `/api/users/{id}/toggle-active` | Admin | Activar / desactivar (borrado lógico) |
| GET | `/api/roles` | Admin | Roles para el alta de usuarios |
| GET | `/api/report-categories` | Personal | Categorías de incidentes |
| POST | `/api/report-categories` | Admin | Nueva categoría |
| GET | `/api/reports` | Personal | Lista (Admin ve todos, el ranger solo los suyos). Filtros: `status`, `categoryId`, `from`, `to` |
| GET | `/api/reports/{id}` | Personal | Detalle |
| POST | `/api/reports` | Ranger/Helper | Crear uno (multipart, foto opcional en `photo`) |
| POST | `/api/reports/sync` | Ranger/Helper | **Bulk insert** de hasta 200 reportes offline (JSON) |
| POST | `/api/reports/{id}/photo` | Dueño o Admin | Subir/reemplazar la foto (multipart `photo`) |
| PATCH | `/api/reports/{id}/status` | Admin | `Pending` · `Done` · `Canceled` |
| GET | `/api/tasks` | Admin | Todas las tareas |
| GET | `/api/tasks/mine` | Personal | Tareas asignadas al usuario |
| POST | `/api/tasks` | Admin | Asignar tarea a un ranger |
| PATCH | `/api/tasks/{id}/status` | Asignado o Admin | Cambiar estado |
| **POST** | **`/api/feedback`** | **Público** | Formulario de turistas (multipart): `portalId`, `kind`, `rating`, `message`, `visitDate`, `email`, `clientCode`, `website` (honeypot), `photo` → `201 { "code": "IB-V1234" }` |
| GET | `/api/tourists` | Admin | Valoraciones. Filtros: `portal=laguna`, `kind=complaint` |
| GET | `/api/tourists/{id}` | Admin | Detalle |

Límites: login 10 intentos/min por IP, feedback público 20/min por IP, requests de hasta 10 MB y fotos de hasta 8 MB (JPG, PNG o WebP, validadas por su contenido real).

## Cómo funciona la sincronización offline

1. En el terreno, la app del ranger guarda cada reporte en el teléfono con un **`clientId` (Guid) generado ahí mismo**.
2. Al recuperar señal manda todos juntos a `POST /api/reports/sync`.
3. El `ReportService` busca qué `clientId` ya existen (reintentos), valida las categorías con **una** consulta e inserta los nuevos con `AddRange` + **un solo** `SaveChanges` dentro de una transacción: entran todos o ninguno.
4. Responde `clientId → reportId` de cada reporte (nuevo o ya existente). Si la señal se corta a mitad de camino y la app reintenta, **no se duplica nada**.
5. Después la app sube cada foto a `POST /api/reports/{reportId}/photo`.

El formulario de turistas usa la misma idea con `clientCode`.

## Fotos: Cloudflare R2

Las fotos **nunca** se guardan en la base: van a un bucket de Cloudflare R2 (compatible con S3) y la DB guarda solo `PhotoKey` y `PhotoUrl`.

- `R2StorageService` usa el SDK de AWS S3 apuntando a `https://<account-id>.r2.cloudflarestorage.com`.
- Si R2 no está configurado y el entorno **no** es Production, se usa `LocalDiskStorageService` (guarda en `wwwroot/uploads` y la API las sirve en `/uploads/...`). En Production la API no arranca sin R2.

## Configuración y secretos

**Nunca** se guardan secretos en `appsettings.json`. En desarrollo van en *User Secrets*; en producción, en variables de entorno (`Seccion__Clave`). Plantilla completa: [`IberaReclamos.API/appsettings.example.json`](IberaReclamos.API/appsettings.example.json).

| Clave | Obligatoria | Ejemplo / descripción |
| --- | --- | --- |
| `ConnectionStrings:DefaultConnection` | Sí | `Host=localhost;Port=5432;Database=RangersDB;Username=postgres;Password=...` |
| `Jwt:Key` | Sí | 32 caracteres o más |
| `Jwt:Issuer` / `Jwt:Audience` / `Jwt:DurationInMinutes` | No | Por defecto `IberaReclamos.API` / `IberaReclamos.Clients` / `720` (12 h) |
| `SeedAdmin:Email` / `SeedAdmin:Password` | Para el primer arranque | Crea el primer Admin si todavía no existe ninguno |
| `R2:Enabled`, `R2:AccountId`, `R2:AccessKeyId`, `R2:SecretAccessKey`, `R2:BucketName`, `R2:PublicBaseUrl` | En producción | Cloudflare R2 |
| `Cors:AllowedOrigins` | No | Por defecto `http://localhost:3000` y `http://localhost:5173` |
| `Database:MigrateOnStartup` | No | `true` en `appsettings.Development.json`: aplica las migraciones al arrancar |

## Correrlo en tu máquina

Requisitos: [.NET SDK 10](https://dotnet.microsoft.com/download), PostgreSQL 16 o superior y la herramienta de EF (`dotnet tool install --global dotnet-ef`).

```bash
cd IberaReclamos.API

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=RangersDB;Username=postgres;Password=TU_PASSWORD"
dotnet user-secrets set "Jwt:Key" "una-clave-larga-de-al-menos-32-caracteres-123456"
dotnet user-secrets set "SeedAdmin:Email" "admin@ibera.local"
dotnet user-secrets set "SeedAdmin:Password" "Admin12345"

cd ..
dotnet ef database update --project IberaReclamos.Infrastructure --startup-project IberaReclamos.API
dotnet run --project IberaReclamos.API --launch-profile http
```

Abrí **http://localhost:5196/swagger**.

> En Development las migraciones también se aplican solas al arrancar (`Database:MigrateOnStartup`), así que el `database update` es opcional.

Nueva migración después de cambiar entidades o configuraciones:

```bash
dotnet ef migrations add NombreDelCambio --project IberaReclamos.Infrastructure --startup-project IberaReclamos.API --output-dir Migrations
```

## Cómo probarlo paso a paso

### 1. Login y alta de un guardaparque (Swagger)

1. Abrí `http://localhost:5196/swagger`.
2. `POST /api/auth/login` con `{ "email": "admin@ibera.local", "password": "Admin12345" }` → copiá el `token`.
3. Botón **Authorize** (arriba a la derecha) → pegá el token (sin la palabra *Bearer*).
4. `GET /api/roles` → copiá el `id` del rol **Ranger** (`8d1f2c3a-0001-4a6e-9b1a-000000000002`).
5. `POST /api/users`:
   ```json
   { "name": "Juan", "lastName": "Pérez", "phoneNumber": "3794123456", "email": "juan@ibera.local",
     "dni": "12345678", "portal": "Cambyreta", "password": "Ranger12345", "idRol": "8d1f2c3a-0001-4a6e-9b1a-000000000002" }
   ```
6. Probá también las validaciones: un DNI `12.345.678`, un email inválido o una contraseña `abc` devuelven `400` con el error de cada campo; repetir el email devuelve `409`.

### 2. Sincronización masiva (como ranger)

1. Logueate como `juan@ibera.local` y autorizá Swagger con su token.
2. `GET /api/report-categories` → copiá el id de *Lixiviados* (`8d1f2c3a-0002-4a6e-9b1a-000000000001`).
3. `POST /api/reports/sync`:
   ```json
   { "reports": [
     { "clientId": "11111111-1111-4111-8111-111111111111", "latitude": -28.52, "longitude": -57.18,
       "notes": "Bidones con líquido", "captureDate": "2026-10-14T15:00:00Z", "reportCategoryId": "8d1f2c3a-0002-4a6e-9b1a-000000000001" }
   ] }
   ```
   → `created: 1`.
4. Mandá **exactamente lo mismo** otra vez → `created: 0, alreadySynced: 1` (idempotente).
5. `POST /api/reports/{id}/photo` con una foto → la respuesta trae `photoUrl`. Abrila en el navegador.
6. Con el token de Admin: `GET /api/reports` y `PATCH /api/reports/{id}/status` con `{ "status": "Done" }`.

### 3. Tareas

1. Como Admin: `POST /api/tasks` con `{ "name": "Retirar bidones", "assignedToId": "<id de Juan>", "reportId": "<id del reporte>" }`.
2. Como Juan: `GET /api/tasks/mine` y `PATCH /api/tasks/{id}/status` con `{ "status": "Done" }`.
3. Como otro ranger, el mismo PATCH devuelve `403`.

### 4. Formulario de turistas

Desde Swagger (`POST /api/feedback`, sin token) o desde IberAPP con `NEXT_PUBLIC_API_URL=http://localhost:5196`:

- `portalId=laguna`, `kind=complaint`, `rating=2`, `message=Los baños estaban sucios`, `clientCode=IB-V1234` → `201 { "code": "IB-V1234" }`.
- Reenviá lo mismo → mismo código y no se duplica.
- `rating=9` o `message=corto` → `400` con los mismos mensajes que muestra el formulario.
- Como Admin: `GET /api/tourists?portal=laguna`.

### 5. Desde la app de guardaparques

Seguí la guía de prueba del [README del frontend](https://github.com/FabianGQuintana/IBERA-Rangers-Client#cómo-probarlo-paso-a-paso).

## Deploy

Plan sugerido: **Railway** (API + PostgreSQL), **Cloudflare R2** (fotos) y **Vercel** (frontends). Pasos y lo que falta antes de publicar: [`docs/DEPLOY.md`](docs/DEPLOY.md).

## Forma de trabajo

- Una rama por tarea (`feature/...`, `fix/...`) → PR hacia `development` → `main` para releases.
- Commits en inglés con [Conventional Commits](https://www.conventionalcommits.org/) (`feat:`, `fix:`, `docs:`, `chore:`).

## Licencia

[MIT](LICENSE)
