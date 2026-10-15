# 🌿 IberaReclamos API

Backend del sistema de reclamos y reportes ambientales del **Gran Parque Iberá** 🐊 (Desafío Iberá · Hackathon).

## 🤔 ¿Qué hace?

- 🧳 **Turistas:** dejan valoraciones, sugerencias o quejas sobre los portales del parque, con foto opcional y sin crear cuenta.
- 🥾 **Guardaparques:** reportan incidentes ambientales (basura, lixiviados, fauna herida…) con foto y ubicación GPS, **aunque no tengan señal**, y los envían todos juntos cuando vuelven a tener conexión.
- 🛡️ **Administradores:** crean usuarios, asignan tareas a los guardaparques y revisan los reportes y las quejas.

## 🧰 Tecnologías

.NET 10 · PostgreSQL · Entity Framework Core · JWT · Cloudflare R2 (fotos) · Swagger

## 🗂️ Cómo está organizado

```
📦 IberaReclamos.API             → la puerta de entrada (controllers)
📦 IberaReclamos.Application     → la lógica del negocio (servicios, validaciones)
📦 IberaReclamos.Domain          → las entidades (usuarios, reportes, tareas…)
📦 IberaReclamos.Infrastructure  → base de datos, fotos y seguridad
```

## 🚀 Cómo levantarlo

1. Tené instalado **.NET 10** y **PostgreSQL**.
2. Cargá tus datos privados (no se suben a GitHub 🔒):

   ```bash
   cd IberaReclamos.API
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=RangersDB;Username=postgres;Password=TU_PASSWORD"
   dotnet user-secrets set "Jwt:Key" "una-clave-larga-de-al-menos-32-caracteres-123456"
   dotnet user-secrets set "SeedAdmin:Email" "admin@ibera.local"
   dotnet user-secrets set "SeedAdmin:Password" "Admin12345"
   ```

3. Arrancá la API (crea la base y el primer admin solita ✨):

   ```bash
   dotnet run --launch-profile http
   ```

4. Abrí 👉 **http://localhost:5196/swagger**

## 🧪 Cómo probarlo

1. 🔑 En Swagger, `POST /api/auth/login` con el admin → copiá el `token` → botón **Authorize** → pegalo.
2. 👤 `POST /api/users` → creá un guardaparque con el rol **Ranger** (lo sacás de `GET /api/roles`).
3. 🥾 Logueate como ese guardaparque y mandá reportes con `POST /api/reports/sync`. Si mandás lo mismo dos veces, **no se duplica** 👌
4. 📷 Subile una foto con `POST /api/reports/{id}/photo`.
5. 🧳 Probá el formulario de turistas con `POST /api/feedback` (no necesita login).
6. 📱 Para probar todo desde la app de los guardaparques, seguí el [README del frontend](https://github.com/FabianGQuintana/IBERA-Rangers-Client).

## ☁️ Deploy

La idea es **Railway** (API + base de datos), **Cloudflare R2** (fotos) y **Vercel** (frontends). Los pasos están en [`docs/DEPLOY.md`](docs/DEPLOY.md).

## 🔗 Proyectos relacionados

- 📱 [IBERA-Rangers-Client](https://github.com/FabianGQuintana/IBERA-Rangers-Client): la app de los guardaparques
- 🌐 [IberAPP](https://github.com/KaibaBenja/IberAPP): la web de los turistas

---

Hecho con 💚 para el Iberá · Licencia [MIT](LICENSE)
