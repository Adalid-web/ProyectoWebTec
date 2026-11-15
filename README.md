# ProyectoTecWeb · Circle Rush

Juego de navegador muy simple: aparecen círculos en pantalla y hay que hacer clic en la mayor cantidad posible antes de que terminen los 30 segundos. Al final se guarda el nombre y el puntaje en una leaderboard de PostgreSQL.

El proyecto también cumple la consigna de Web API: cada servidor/país tiene su propia tabla y su propio controlador con CRUD completo.

## Integrantes y tablas

| Integrante | Servidor | Tabla | Controlador | Estado |
|---|---|---|---|---|
| Leonardo Julio | Bolivia 🇧🇴 | `leaderboard_bolivia` | `LeaderboardBoliviaController` | Implementado |
| Jheferson Velasco | Perú 🇵🇪 | `leaderboard_peru` | `LeaderboardPeruController` | Asignado |
| Adalid Calizaya | Argentina 🇦🇷 | `leaderboard_argentina` | `LeaderboardArgentinaController` | Asignado |

> Este repositorio contiene por ahora la implementación de Leonardo Julio (Bolivia). Los otros dos controladores se integran con el mismo contrato descrito más abajo.

## Stack

- .NET 10 Web API con controladores.
- Entity Framework Core 10 + Npgsql.
- PostgreSQL 16.
- Docker + Docker Compose.
- HTML, CSS y JavaScript sin frameworks para el juego.
- Swagger para probar la API.

## Estructura

```text
ProyectoTecWeb/
├─ contructor.yml          # Compose que construye la imagen desde el Dockerfile
├─ despliegue.yml          # Compose que usa la imagen publicada en Docker Hub
├─ .env.example
├─ .gitignore
├─ README.md
├─ scripts/
│  └─ test-crud.ps1
└─ ProyectoTecWeb/
   ├─ ProyectoTecWeb.csproj
   ├─ Program.cs
   ├─ Dockerfile
   ├─ .dockerignore
   ├─ Controllers/
   ├─ Data/
   ├─ DTOs/
   ├─ Models/
   ├─ Migrations/
   └─ wwwroot/             # Juego (index.html, CSS y JS)
```

## Requisitos

- Docker Desktop con Docker Compose.
- Para desarrollo local: .NET 10 SDK.
- Para generar migraciones: `dotnet tool install --global dotnet-ef --version 10.0.15`.

## Configuración

1. Copiar el archivo de ejemplo:

   ```powershell
   Copy-Item .env.example .env
   ```

2. Editar `.env` y cambiar `POSTGRES_PASSWORD`. El archivo `.env` está ignorado por Git.

Variables principales:

| Variable | Uso |
|---|---|
| `API_PORT` | Puerto publicado en el host para la API |
| `DOCKERHUB_USER` | Usuario de Docker Hub |
| `IMAGE_NAME` | Nombre de la imagen |
| `IMAGE_TAG` | Etiqueta de la imagen |
| `POSTGRES_HOST` | Host de PostgreSQL; dentro de Compose es `postgres` |
| `POSTGRES_DB` | Nombre de la base |
| `POSTGRES_USER` | Usuario de PostgreSQL |
| `POSTGRES_PASSWORD` | Contraseña de PostgreSQL |

La cadena de conexión se construye con esas variables y se inyecta como `ConnectionStrings__DefaultConnection`. No hay credenciales dentro de la imagen.

## Ejecutar con construcción desde el código

Desde la raíz del proyecto:

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

## Ejecutar el despliegue con la imagen publicada

Primero detener el stack de construcción si está corriendo, porque ambos usan el mismo puerto y los mismos nombres de contenedor:

```powershell
docker compose --env-file .env -f contructor.yml down
docker compose --env-file .env -f despliegue.yml pull
docker compose --env-file .env -f despliegue.yml up -d
docker compose --env-file .env -f despliegue.yml ps
```

Probar:

```powershell
curl.exe http://localhost:8080/health
curl.exe http://localhost:8080/api/leaderboard/bolivia?limit=10
```

Este archivo no tiene sección `build`: usa directamente `nardolj/proyectowebtec:1.0.0` desde Docker Hub.

## Docker Hub

- Repositorio: `https://hub.docker.com/r/nardolj/proyectowebtec`
- Etiqueta exacta: `1.0.0`
- También se publica `latest`.
- Descarga:

  ```powershell
  docker pull nardolj/proyectowebtec:1.0.0
  ```

## Git

- Repositorio: `https://github.com/NardoLj/ProyectoWebTec`

## Endpoints de Bolivia (Leonardo Julio)

Base: `/api/leaderboard/bolivia`

| Método | Ruta | Descripción |
|---|---|---|
| `POST` | `/api/leaderboard/bolivia` | Crea una puntuación |
| `GET` | `/api/leaderboard/bolivia?limit=10` | Lista las mejores puntuaciones |
| `GET` | `/api/leaderboard/bolivia/{id}` | Obtiene una puntuación por ID |
| `PUT` | `/api/leaderboard/bolivia/{id}` | Actualiza una puntuación |
| `DELETE` | `/api/leaderboard/bolivia/{id}` | Elimina una puntuación |

Ejemplo de creación:

```powershell
curl.exe -X POST http://localhost:8080/api/leaderboard/bolivia `
  -H "Content-Type: application/json" `
  -d '{\"playerName\":\"Leonardo\",\"score\":25}'
```

La tabla tiene las columnas:

| Columna | Tipo | Regla |
|---|---|---|
| `id` | integer | PK, identity |
| `player_name` | varchar(30) | obligatorio |
| `score` | integer | obligatorio, `>= 0` |
| `created_at` | timestamptz | por defecto `NOW()` |

La migración se genera con:

```powershell
cd ProyectoTecWeb
dotnet ef migrations add InitialCreate
```

Al arrancar, la API ejecuta `Database.Migrate()` y crea la tabla si no existe.

## Contrato para integrar los otros servidores

Para que el frontend funcione sin cambios, cada compañero debe mantener este contrato:

- Tabla: `leaderboard_peru` y `leaderboard_argentina`.
- Ruta: `/api/leaderboard/peru` y `/api/leaderboard/argentina`.
- Campos JSON: `playerName`, `score`; respuesta con `id`, `playerName`, `score`, `createdAt`.
- Endpoints: `POST`, `GET` lista, `GET /{id}`, `PUT /{id}`, `DELETE /{id}`.
- Registrar la entidad en `AppDbContext` y crear su configuración en `Data/Configurations`.
- Agregar su migración con `dotnet ef migrations add Add<Servidor>Leaderboard`.

El selector del juego ya apunta a esas rutas; si el controlador no existe, muestra “Servidor aún no disponible” sin romper el juego.

## Pruebas CRUD

Con el stack levantado:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-crud.ps1
```

El script prueba `POST`, `GET` lista, `GET` por ID, `PUT`, `DELETE` y verifica el 404 después de borrar.

También se puede usar Swagger en `http://localhost:8080/swagger`.

## Uso de IA

Para el archivo `despliegue.yml` se consultó a una IA con este objetivo:

> Generar un Compose de despliegue para una API .NET 10 publicada como `nardolj/proyectowebtec:1.0.0` y PostgreSQL 16, usando una red compartida, volumen nombrado, healthcheck y variables desde `.env`, sin compilar desde el código fuente.

La propuesta se verificó así:

1. `docker compose --env-file .env -f despliegue.yml config` para validar la sintaxis y la interpolación.
2. `docker compose --env-file .env -f despliegue.yml pull` para descargar la imagen publicada.
3. `docker compose --env-file .env -f despliegue.yml up -d` y `docker compose ps` para confirmar ambos contenedores.
4. `curl.exe http://localhost:8080/health` y una petición al leaderboard para confirmar que la API responde.
5. Revisión de logs y de que no existe la clave `build` en `despliegue.yml`.

## Seguridad

- `.env` no se sube a Git y está excluido de la imagen por `.dockerignore`.
- La cadena de conexión se pasa solo en tiempo de ejecución.
- No se guardan contraseñas en el Dockerfile ni en `appsettings.json`.
