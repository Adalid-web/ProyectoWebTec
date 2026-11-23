# ProyectoTecWeb · Circle Rush

Juego web de 30 segundos: aparecen círculos en pantalla y hay que hacer clic en la mayor cantidad posible. Al final se guarda el puntaje en PostgreSQL y se muestra un leaderboard por país.

El proyecto también cumple la consigna de Web API: cada integrante tiene **cuatro tablas**, **dos proveedores de login** y controladores con operaciones protegidas por sesión.

- Repositorio base: <https://github.com/NardoLj/ProyectoWebTec>
- Guía de contribución: [CONTRIBUTING.md](CONTRIBUTING.md)
- Guía de autenticación: [docs/autenticacion.md](docs/autenticacion.md)
- Plantilla de evidencias: [docs/plantilla-evidencias.md](docs/plantilla-evidencias.md)
- Descripciones de PR: [docs/descripciones-pr.md](docs/descripciones-pr.md)

## Integrantes, tablas y proveedores

| Integrante | País | Cuatro tablas | Login fácil | Login medio | Estado |
|---|---|---|---|---|---|
| **Leonardo Julio** (líder) | Bolivia 🇧🇴 | `leaderboard_bolivia`, `jugadores_bolivia`, `partidas_bolivia`, `logros_bolivia` | Discord | Google | En integración |
| **Jheferson Velasco** | Perú 🇵🇪 | `leaderboard_peru`, `jugadores_peru`, `partidas_peru`, `logros_peru` | Twitch | Microsoft | Pendiente |
| **Adalid Calizaya** | Argentina 🇦🇷 | `leaderboard_argentina`, `jugadores_argentina`, `partidas_argentina`, `logros_argentina` | Dropbox | Slack | Tablas integradas |

El reparto está registrado antes de programar y no se repite ningún proveedor entre integrantes.

> Adalid ya integró sus cuatro tablas de Argentina mediante la PR #1. Las demás etapas se trabajan en ramas separadas y se integran por Pull Request.

## Stack

- .NET 10 Web API con controladores.
- Entity Framework Core 10 + Npgsql.
- PostgreSQL 16.
- Cookie de sesión + OAuth con Google y Discord (los compañeros agregan sus proveedores).
- HTML, CSS y JavaScript sin frameworks para el juego.
- Docker + Docker Compose.
- Swagger para documentar y probar la API.

## Estructura

```text
ProyectoTecWeb/
├─ contructor.yml              # Compose que construye la imagen desde el Dockerfile
├─ despliegue.yml              # Compose que usa la imagen publicada en Docker Hub
├─ CONTRIBUTING.md             # Flujo de forks, ramas, commits, PR y revisión
├─ .env.example                # Plantilla de variables; no contiene secretos
├─ .gitignore
├─ README.md
├─ docs/
│  ├─ autenticacion.md         # Configuración de Google, Discord y proveedores futuros
│  ├─ plantilla-evidencias.md  # Guía para el PDF/Word individual
│  └─ evidencias.pdf           # Evidencias de la entrega anterior
├─ scripts/
│  └─ test-crud.ps1
└─ ProyectoTecWeb/
   ├─ ProyectoTecWeb.csproj
   ├─ Program.cs
   ├─ Dockerfile
   ├─ .dockerignore
   ├─ Auth/                    # Configuración de cookie y proveedores OAuth
   ├─ Controllers/
   ├─ Data/
   │  ├─ AppDbContext.cs
   │  └─ Configurations/
   ├─ DTOs/
   ├─ Models/
   ├─ Migrations/
   └─ wwwroot/                 # Juego (index.html, CSS y JS)
```

## Requisitos

- Docker Desktop con Docker Compose.
- Para desarrollo local: .NET 10 SDK.
- Para migraciones: `dotnet tool install --global dotnet-ef --version 10.0.15`.

## Configuración del entorno

1. Copiar la plantilla:

   ```powershell
   Copy-Item .env.example .env
   ```

2. Editar `.env` y cambiar como mínimo `POSTGRES_PASSWORD`.
3. Completar las credenciales OAuth de los proveedores que se van a probar. La guía detallada está en [docs/autenticacion.md](docs/autenticacion.md).

Variables principales:

| Variable | Uso |
|---|---|
| `API_PORT` | Puerto publicado en el host para la API |
| `APP_PUBLIC_URL` | URL pública usada para documentar los callbacks |
| `DOCKERHUB_USER` | Usuario de Docker Hub |
| `IMAGE_NAME` | Nombre de la imagen |
| `IMAGE_TAG` | Etiqueta de la imagen |
| `POSTGRES_HOST` | Host de PostgreSQL; dentro de Compose es `postgres` |
| `POSTGRES_DB` | Nombre de la base |
| `POSTGRES_USER` | Usuario de PostgreSQL |
| `POSTGRES_PASSWORD` | Contraseña de PostgreSQL |
| `GOOGLE_CLIENT_ID` / `GOOGLE_CLIENT_SECRET` | Credenciales de Google |
| `DISCORD_CLIENT_ID` / `DISCORD_CLIENT_SECRET` | Credenciales de Discord |

Las credenciales se inyectan en tiempo de ejecución como `Authentication__Google__ClientId`, `Authentication__Google__ClientSecret`, etc. No se hornean en la imagen.

## Ejecutar con construcción desde el código

```powershell
docker compose --env-file .env -f contructor.yml up --build -d
docker compose --env-file .env -f contructor.yml ps
```

Abrir:

- Juego: `http://localhost:8080/`
- Swagger: `http://localhost:8080/swagger`
- Health: `http://localhost:8080/health`
- Health de base de datos: `http://localhost:8080/health/db`

Detener sin borrar datos:

```powershell
docker compose --env-file .env -f contructor.yml down
```

Borrar también el volumen de PostgreSQL:

```powershell
docker compose --env-file .env -f contructor.yml down -v
```

## Ejecutar con la imagen publicada

Primero detener el stack de construcción, porque ambos usan el mismo puerto y los mismos nombres de contenedor:

```powershell
docker compose --env-file .env -f contructor.yml down
docker compose --env-file .env -f despliegue.yml pull
docker compose --env-file .env -f despliegue.yml up -d
docker compose --env-file .env -f despliegue.yml ps
```

`despliegue.yml` no tiene sección `build`: usa una imagen publicada en Docker Hub.

> La etiqueta `1.0.0` corresponde a la entrega anterior. Cuando se integren las nuevas funcionalidades de login y soft delete se publicará una etiqueta nueva, por ejemplo `1.1.0`, y se actualizará `.env`.

## Ejecutar en local sin Docker

Tener PostgreSQL disponible y definir la cadena de conexión y las credenciales OAuth:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=proyectotecweb;Username=proyectotecweb;Password=tu-clave"
$env:Authentication__Google__ClientId = "tu-client-id.apps.googleusercontent.com"
$env:Authentication__Google__ClientSecret = "tu-client-secret"
$env:Authentication__Discord__ClientId = "tu-discord-client-id"
$env:Authentication__Discord__ClientSecret = "tu-discord-client-secret"

dotnet run --project .\ProyectoTecWeb
```

También se puede usar `dotnet user-secrets`; los pasos están en [docs/autenticacion.md](docs/autenticacion.md).

## Autenticación

La aplicación usa **cookie de sesión + OAuth**. El flujo es:

1. El usuario hace clic en `Continuar con Google` o `Continuar con Discord`.
2. La API redirige al proveedor.
3. El proveedor vuelve a `/signin-google` o `/signin-discord`.
4. La API crea la cookie de sesión y redirige a `/`.
5. El frontend consulta `/api/auth/me` y muestra la identidad.

Endpoints de sesión:

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/auth/providers` | Proveedores configurados |
| `GET` | `/api/auth/me` | Identidad de la sesión; 401 sin sesión |
| `POST` | `/logout` | Cierra la sesión |

Callbacks locales:

```text
http://localhost:8080/signin-google
http://localhost:8080/signin-discord
```

Los proveedores que no tengan credenciales configuradas no se registran y su botón no se muestra; la aplicación arranca igual.

## Endpoints de Bolivia

Todos los endpoints de las cuatro tablas requieren sesión. Sin cookie válida responden **401**.

### Leaderboard

Base: `/api/leaderboard/bolivia`

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/leaderboard/bolivia?limit=10` | Lista los mejores puntajes |
| `GET` | `/api/leaderboard/bolivia/{id}` | Obtiene un puntaje por ID |
| `POST` | `/api/leaderboard/bolivia` | Crea un puntaje |
| `PUT` | `/api/leaderboard/bolivia/{id}` | Actualiza un puntaje |
| `DELETE` | `/api/leaderboard/bolivia/{id}` | Soft delete del puntaje |

### Jugadores

Base: `/api/bolivia/jugadores`

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/bolivia/jugadores` | Lista jugadores activos |
| `GET` | `/api/bolivia/jugadores/{id}` | Obtiene un jugador activo |
| `POST` | `/api/bolivia/jugadores` | Crea un jugador |
| `PUT` | `/api/bolivia/jugadores/{id}` | Actualiza un jugador |
| `DELETE` | `/api/bolivia/jugadores/{id}` | Soft delete del jugador |

### Partidas

Base: `/api/bolivia/partidas`

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/bolivia/partidas` | Lista partidas activas |
| `GET` | `/api/bolivia/partidas/{id}` | Obtiene una partida activa |
| `POST` | `/api/bolivia/partidas` | Crea una partida |
| `PUT` | `/api/bolivia/partidas/{id}` | Actualiza una partida |
| `DELETE` | `/api/bolivia/partidas/{id}` | Soft delete de la partida |

### Logros

Base: `/api/bolivia/logros`

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/bolivia/logros` | Lista logros activos |
| `GET` | `/api/bolivia/logros/{id}` | Obtiene un logro activo |
| `POST` | `/api/bolivia/logros` | Crea un logro |
| `PUT` | `/api/bolivia/logros/{id}` | Actualiza un logro |
| `DELETE` | `/api/bolivia/logros/{id}` | Soft delete del logro |

## Soft delete

Las cuatro tablas de cada integrante tienen las columnas:

- `is_deleted` (`boolean`, por defecto `false`).
- `deleted_at` (`timestamptz`, nulo mientras está activo).

Reglas:

- Los GET excluyen los registros con `is_deleted = true`.
- `DELETE` no borra la fila: marca `is_deleted = true` y asigna `deleted_at`.
- Un registro inactivo no se puede actualizar ni eliminar otra vez; la API responde 404.
- La fila permanece físicamente en PostgreSQL y se puede comprobar con SQL.

Ejemplo de verificación:

```sql
SELECT id, nombre, is_deleted, deleted_at
FROM jugadores_bolivia
ORDER BY id DESC;
```

## Migraciones

La API ejecuta `Database.Migrate()` al arrancar. Para generar una migración:

```powershell
cd ProyectoTecWeb
dotnet ef migrations add NombreDeLaMigracion
```

Reglas para evitar conflictos:

- Sincronizar `main` antes de generar la migración.
- No editar a mano `AppDbContextModelSnapshot.cs` salvo que sea necesario.
- Si hay conflicto en el snapshot, regenerar la migración sobre la versión más reciente.

## Pruebas

Con el stack levantado, el script puede comprobar primero el rechazo sin sesión:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-crud.ps1
```

Para probar el CRUD con sesión, iniciar sesión en el navegador, copiar la cookie `ProyectoTecWeb.Auth` desde DevTools y pasarla al script:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-crud.ps1 -Cookie "ProyectoTecWeb.Auth=VALOR"
```

No pegar la cookie en capturas, README, PDF ni ZIP. El script comprueba POST, GET lista, GET por ID, PUT, DELETE lógico, 404 posterior y que un registro inactivo ya no se pueda actualizar ni eliminar.

Pruebas mínimas:

1. `GET /api/leaderboard/bolivia` sin cookie → 401.
2. Login con Google → `GET /api/auth/me` → 200.
3. `POST` de leaderboard con sesión → 201.
4. Login con Discord → `GET /api/auth/me` → 200.
5. `DELETE` → 204; `GET` por ID → 404; fila conservada en PostgreSQL con `is_deleted = true`.
6. Logout → `GET /api/auth/me` → 401.

## Flujo de ramas y PRs

El detalle completo está en [CONTRIBUTING.md](CONTRIBUTING.md). Resumen:

| Etapa | Rama | Contenido |
|---|---|---|
| 0 | `docs/equipo` | README, CONTRIBUTING y reparto |
| 1 | `feat/<pais>-tablas` | Cuatro tablas + migración |
| 2 | `feat/<pais>-auth` | Dos logins + botones + sesión |
| 3 | `feat/<pais>-controllers` | POST/GET protegidos con `[Authorize]` |
| 4 | `practice/<pais>-post-duplicado` | PR fallida, cerrada sin merge |
| 5 | `feat/<pais>-put-softdelete` | Dos commits: PUT y soft delete; Squash and merge |

Nadie trabaja directamente en `main`. Los compañeros trabajan desde su fork y el líder revisa e integra. En las PR del líder, otro integrante deja la revisión y el líder hace la integración.

## Docker Hub

- Repositorio: <https://hub.docker.com/r/nardolj/proyectowebtec>
- Etiqueta de la entrega anterior: `1.0.0`.
- También se publicó `latest`.
- La nueva versión con autenticación se publicará después de integrar y probar las ramas.

## Evidencias y entregables

Cada integrante entrega su propio PDF o Word con:

- Nombre, equipo y rol.
- Cuatro tablas y dos proveedores.
- Enlaces al repositorio base, su fork y sus PR.
- Capturas de login, identidad, logout y endpoints.
- Captura de la PR fallida y su rechazo.
- Captura de los dos commits y del squash final.

El ZIP final se genera desde la versión integrada de `main` y no debe incluir `.env`, `bin/` ni `obj/`.

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\crear-entrega.ps1
```

El script copia el proyecto a una carpeta temporal, excluye `.git`, `.env`, `bin`, `obj`, evidencias antiguas y archivos ZIP, y agrega un `LEEME.txt` con los pasos de ejecución.

## Seguridad

- `.env` está ignorado por Git y no se incluye en la imagen.
- Las credenciales OAuth se pasan solo en tiempo de ejecución.
- No se guardan contraseñas en el Dockerfile ni en `appsettings.json`.
- Las capturas y documentos deben ocultar secretos.
- No agregar integrantes como colaboradores del repositorio base.

## Uso de IA

Se usó IA como apoyo para la estructura del proyecto, el juego, la configuración de Docker y la documentación. El código generado se revisa, compila y prueba antes de integrarse.
