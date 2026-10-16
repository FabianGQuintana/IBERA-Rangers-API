# 🚀 Deploy paso a paso: Cloudflare R2 + Railway + Vercel

```
 🧳 Turistas ──► IberAPP (Vercel) ───────────────┐
                                                 ▼
 🥾 Rangers ──► IBERA-Rangers-Client (Vercel) ──► IberaReclamos API (Railway) ──► PostgreSQL (Railway)
                                                 └──► Cloudflare R2 (fotos)
```

Hacelo **en este orden**: cada paso necesita una URL o una clave del anterior. 🔒 Las claves se cargan solo en los paneles, nunca en el código ni en el chat.

---

## 1. ☁️ Cloudflare R2 (fotos)

1. [dash.cloudflare.com](https://dash.cloudflare.com) → **R2 Object Storage** → **Create bucket** → nombre `ibera-reclamos`.
2. Dentro del bucket → **Settings → Public access → R2.dev subdomain → Allow**. Copiá la URL pública (`https://pub-xxxx.r2.dev`).
3. Volvé a **R2** → **Manage R2 API Tokens** → **Create API token**:
   - Permisos: **Object Read & Write**
   - Bucket: solo `ibera-reclamos`
4. Anotá en un lugar seguro: **Account ID**, **Access Key ID** y **Secret Access Key** (el Secret se ve una sola vez).

## 2. 🚂 Railway (API + base de datos)

1. [railway.com](https://railway.com) → **New Project → Deploy from GitHub repo** → `IBERA-Rangers-API` (rama `main`). Railway detecta el `Dockerfile` y `railway.json` solo.
2. En el mismo proyecto: **+ Create → Database → PostgreSQL**.
3. Servicio de la API → **Variables** → **Raw Editor** → pegá esto y completá lo que está entre `< >`:

   ```env
   DATABASE_URL=${{Postgres.DATABASE_URL}}
   Jwt__Key=<64 caracteres al azar>
   Database__MigrateOnStartup=true
   SeedAdmin__Email=<tu email de admin>
   SeedAdmin__Password=<contraseña del primer admin>
   R2__Enabled=true
   R2__AccountId=<Account ID>
   R2__AccessKeyId=<Access Key ID>
   R2__SecretAccessKey=<Secret Access Key>
   R2__BucketName=ibera-reclamos
   R2__PublicBaseUrl=<https://pub-xxxx.r2.dev>
   Cors__AllowedOrigins__0=<URL de IberAPP en Vercel, se completa en el paso 3>
   ```

   > 💡 Para la `Jwt__Key` podés usar el botón **Generate** de Railway, o en PowerShell: `[Convert]::ToBase64String((1..48 | % {Get-Random -Max 256}))`

4. API → **Settings → Networking → Generate Domain**. Te da algo como `https://ibera-rangers-api-production.up.railway.app`.
5. Probá: `https://<tu-dominio>/health` tiene que responder **Healthy** ✅

## 3. ▲ Vercel (frontends)

### 🥾 App de guardaparques

1. [vercel.com](https://vercel.com) → **Add New → Project** → `IBERA-Rangers-Client`.
2. **Environment Variables**: `API_URL` = la URL de Railway (sin `/` al final).
3. **Deploy**.

### 🧳 IberAPP (turistas)

1. **Add New → Project** → `IberAPP`.
2. **Environment Variables**: `NEXT_PUBLIC_API_URL` = la URL de Railway.
3. **Deploy** → copiá la URL que te da Vercel (por ejemplo `https://iberapp.vercel.app`).
4. Volvé a Railway → `Cors__AllowedOrigins__0` = esa URL → la API se reinicia sola.

> ⚠️ Si cambiás la URL de la API, en Vercel hay que hacer **Redeploy**: las dos apps la leen al compilar.

## 4. ✅ Prueba final

- [ ] `https://<api>/health` → Healthy
- [ ] En la app de guardaparques entrás con el admin de `SeedAdmin`
- [ ] Creás un ranger (por ahora con Postman o curl: `POST /api/users`, Swagger solo funciona en local)
- [ ] Desde el celular: login con el ranger → reporte con foto → **Sincronizar** ✅
- [ ] En IberAPP: **Valorar** con foto → llega el código `IB-V…` ✅
- [ ] Las fotos se abren desde la URL de R2

## 🔧 Si algo falla

| Síntoma | Causa probable |
| --- | --- |
| Railway: el deploy queda en *unhealthy* | Mirá **Deployments → Logs**. Lo más común: falta `Jwt__Key` (32+ caracteres) o la variable de R2 |
| `R2 must be configured in Production` | Falta alguna variable `R2__*` o `R2__Enabled` no es `true` |
| IberAPP: "No pudimos enviar tu valoración" | Falta su URL en `Cors__AllowedOrigins__0` o `NEXT_PUBLIC_API_URL` está mal |
| App de rangers: "Sin conexión con el servidor" | `API_URL` mal escrita o no se hizo Redeploy después de cambiarla |
| Las fotos no cargan | `R2__PublicBaseUrl` no es la URL pública del bucket, o el acceso público está desactivado |

## 📌 Pendiente del producto (no bloquea el deploy)

- 🛡️ Panel web de administración (crear rangers, ver reportes en un mapa, asignar tareas)
- 📋 Tareas asignadas dentro de la app del ranger
- 🔄 Refresh token (hoy la sesión dura 12 h)
- 🧪 Tests automáticos y CI
