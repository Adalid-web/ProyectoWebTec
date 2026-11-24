# Proyecto Final - Punto 4: Docker Compose de desarrollo

## Proyecto
Circle Rush - Tecnologia Web I

## Objetivo
Levantar el backend ASP.NET Core 10 y PostgreSQL 16
mediante Docker Compose.

## Archivo principal
compose.dev.yml

## Servicios
- api: backend ASP.NET Core 10.
- postgres: base de datos PostgreSQL 16.

## Configuracion
- Puerto interno de la API: 8080.
- Puerto HTTPS local: 8443.
- Red interna: appnet.
- Volumen persistente: pgdata.
- Credenciales: archivo .env privado.
- Migraciones: Entity Framework Core.

El Compose construye la imagen localmente.

El proceso ASP.NET Core se ejecuta con
ASPNETCORE_ENVIRONMENT=Production para evitar un
problema con el manifiesto de archivos estaticos
del contenedor actual.

## Iniciar

docker compose --env-file .env \
  -f compose.dev.yml \
  -f oauth.local.yml \
  up -d --build

## Verificar servicios

docker compose --env-file .env \
  -f compose.dev.yml \
  -f oauth.local.yml \
  ps

## Verificar PostgreSQL

curl -k -i https://localhost:8443/health/db

Resultado comprobado: HTTP 200.

## Verificar autorizacion

curl -k -i https://localhost:8443/api/argentina/jugadores

Resultado comprobado sin sesion: HTTP 401.

## Migraciones
El backend ejecuta Database.Migrate() durante
su inicio para aplicar las migraciones pendientes.

## Persistencia
PostgreSQL utiliza el volumen pgdata para
conservar los datos.

No ejecutar docker compose down -v si se
desea conservar la base de datos.

## Seguridad
El archivo .env y los certificados privados
no deben subirse a GitHub.
