# Deploy: Railway + Cloudflare R2 + Vercel

```
 Turistas ──► IberAPP (Vercel) ──────────────┐  fetch directo (CORS)
                                             ▼
 Rangers ──► IBERA-Rangers-Client (Vercel) ──► IberaReclamos API (Railway) ──► PostgreSQL (Railway)
             /api/* reenviado por rewrite        │
                                                 └──► Cloudflare R2 (fotos) ──► URL pública de las fotos
```

## Estado actual

| Pieza | Lista para producción | Detalle |
| --- | --- | --- |
| Código del backend | Casi | Funciona y está probado de punta a punta, pero faltan los ajustes de la sección **1** para correr detrás del proxy de Railway |
| Base de datos | Sí | Las migraciones de EF crean todo, roles y categorías incluidos |
| Fotos en R2 | Sí (código) | Falta crear el bucket y las credenciales |
| Frontend de rangers | Casi | Faltan los íconos PNG de la PWA (sección **4**) |
| IberAPP (turistas) | Sí | Solo hay que apuntar `NEXT_PUBLIC_API_URL` a Railway |

---

## 1. Cambios de código que faltan antes de publicar (backend)

Son chicos, pero **sin ellos el deploy falla o queda inseguro**:

1. **Forwarded headers.** Railway termina el HTTPS en su proxy y le pasa la request a la API por HTTP. Hay que agregar `app.UseForwardedHeaders()` (con `XForwardedFor | XForwardedProto`) al principio del pipeline. Sin esto:
   - `UseHttpsRedirection()` puede entrar en un bucle de redirecciones (o hay que desactivarlo en Production, como en Versori),
   - **el rate limiting ve a todos los usuarios con la IP del proxy**: el límite de 10 logins por minuto sería para todo el país junto.
2. **Puerto.** Railway asigna el puerto en la variable `PORT`. Configurar `ASPNETCORE_URLS=http://0.0.0.0:${PORT}` (o `ASPNETCORE_HTTP_PORTS`).
3. **Health check.** Agregar `builder.Services.AddHealthChecks()` y `app.MapHealthChecks("/health")` para que Railway sepa cuándo la API está lista.
4. **Dockerfile** (recomendado). Railway puede detectar .NET solo, pero un `Dockerfile` multi-stage (`sdk:10.0` para compilar, `aspnet:10.0` para correr) hace el build predecible.
5. **Cadena de conexión.** Railway da la base como URL (`postgresql://user:pass@host:port/db`), pero Npgsql espera `Host=...;Port=...;Database=...;Username=...;Password=...;SSL Mode=Require`. Armala a mano en la variable `ConnectionStrings__DefaultConnection` usando las variables `PGHOST`, `PGPORT`, etc. del servicio de Postgres, o agregá una conversión en `Program.cs`.
6. **CORS.** IberAPP llama a la API directo desde el navegador, así que su dominio tiene que estar en `Cors__AllowedOrigins__0`. La app de rangers no lo necesita porque pasa por el rewrite de Next.

## 2. Railway (API + PostgreSQL)

1. Nuevo proyecto → **Deploy from GitHub** → `IBERA-Rangers-API`, rama `main`.
2. **Add service → Database → PostgreSQL.**
3. Variables del servicio de la API:

   | Variable | Valor |
   | --- | --- |
   | `ASPNETCORE_ENVIRONMENT` | `Production` |
   | `ConnectionStrings__DefaultConnection` | `Host=${{Postgres.PGHOST}};Port=${{Postgres.PGPORT}};Database=${{Postgres.PGDATABASE}};Username=${{Postgres.PGUSER}};Password=${{Postgres.PGPASSWORD}};SSL Mode=Require;Trust Server Certificate=true` |
   | `Jwt__Key` | Clave aleatoria de 64 caracteres (`openssl rand -base64 48`) |
   | `SeedAdmin__Email` / `SeedAdmin__Password` | Primer admin (después de crearlo podés borrarlas) |
   | `Database__MigrateOnStartup` | `true` (o correr `dotnet ef database update` a mano contra la base de Railway) |
   | `R2__Enabled` = `true`, `R2__AccountId`, `R2__AccessKeyId`, `R2__SecretAccessKey`, `R2__BucketName`, `R2__PublicBaseUrl` | Ver sección 3 |
   | `Cors__AllowedOrigins__0` | `https://iberapp.vercel.app` (dominio real de IberAPP) |

4. **Settings → Networking → Generate domain** → por ejemplo `https://ibera-api.up.railway.app`.
5. Probar con `GET /health` y un `POST /api/auth/login`. Swagger está apagado en Production a propósito.

## 3. Cloudflare R2 (fotos)

1. Cloudflare → **R2 → Create bucket** → `ibera-reclamos`.
2. **Acceso público** del bucket: activá el subdominio `r2.dev` (para la hackathon alcanza) o conectá un dominio propio (recomendado: cache y sin límites de r2.dev). Esa URL es `R2__PublicBaseUrl`.
3. **R2 → Manage API tokens → Create API token** con permiso *Object Read & Write* solo sobre ese bucket → te da `Access Key ID` y `Secret Access Key`.
4. El **Account ID** está en el panel de R2.
5. No hace falta configurar CORS en el bucket: las fotos se suben desde la API, no desde el navegador.

## 4. Vercel (frontends)

### IBERA-Rangers-Client

1. **Import project** → `IBERA-Rangers-Client`, framework Next.js.
2. Variable de entorno **`API_URL=https://ibera-api.up.railway.app`**. Se usa **al compilar** (rewrites), así que si cambia hay que hacer *Redeploy*.
3. **Falta antes de publicar:** íconos PNG de 192×192 y 512×512 (y uno `maskable`) en `app/manifest.ts`. Android no ofrece "Instalar app" solo con el SVG actual.
4. Vercel sirve todo por HTTPS, así que la cámara, el GPS y el service worker funcionan en el celular.
5. Las fotos se comprimen en el teléfono a ~300 KB, muy por debajo del límite de tamaño de los rewrites de Vercel.

### IberAPP (turistas)

- `NEXT_PUBLIC_API_URL=https://ibera-api.up.railway.app` y agregar su dominio a `Cors__AllowedOrigins` en Railway.

## 5. Lo que falta del producto (no bloquea el deploy)

| Prioridad | Qué | Por qué |
| --- | --- | --- |
| Alta | **Panel de administración** (web) | Hoy los usuarios, tareas y estados se manejan desde Swagger o la API. Hace falta una pantalla para crear rangers, ver los reportes en un mapa, asignar tareas y leer las quejas de turistas. |
| Alta | **Tareas en la app del ranger** | El backend ya tiene `GET /api/tasks/mine` y `PATCH /api/tasks/{id}/status`, pero la app todavía no las muestra. |
| Media | **Refresh token** | El token dura 12 h. Si vence en el terreno, el ranger sigue trabajando offline, pero tiene que volver a loguearse para sincronizar. |
| Media | **Tests automáticos** y CI (GitHub Actions: build + tests en cada PR) | Hoy todo se probó a mano. |
| Media | **Íconos PWA PNG** | Ver sección 4. |
| Baja | Borrado de fotos huérfanas en R2, paginación de listados, logs centralizados | Para cuando crezca el volumen. |

## Checklist final

- [ ] Cambios de la sección 1 mergeados en `main`
- [ ] Bucket R2 creado con acceso público y token
- [ ] Railway: Postgres + API con todas las variables, dominio generado y `/health` en 200
- [ ] Primer admin creado y login probado
- [ ] Vercel: Rangers Client con `API_URL` e IberAPP con `NEXT_PUBLIC_API_URL`
- [ ] Dominio de IberAPP agregado a CORS
- [ ] Prueba completa desde un celular real: login → reporte sin señal → sincronizar → foto visible
