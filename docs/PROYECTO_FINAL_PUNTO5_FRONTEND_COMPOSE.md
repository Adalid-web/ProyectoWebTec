# Proyecto Final - Punto 5: frontend independiente en Compose

**Proyecto:** Circle Rush / ProyectoTecWeb

## Arquitectura

- `frontend`: Nginx, publica `https://localhost:8443/`, sirve HTML/CSS/JS y actua como proxy inverso para login, logout, Swagger y API.
- `api`: ASP.NET Core 10, sirve internamente HTTP 8080 y se conecta a PostgreSQL. El puerto de depuracion `${API_PORT}` solo queda accesible desde `127.0.0.1`.
- `postgres`: PostgreSQL 16 con volumen persistente `pgdata`.

Las peticiones del navegador a `/api/*`, `/login/*`, `/signin-*`, `/logout`, `/health/*` y `/swagger/*` son enviadas por Nginx a `api:8080`. No es necesario configurar CORS entre frontend y backend porque comparten el mismo origen HTTPS.

La aplicacion lee `X-Forwarded-Proto` para construir correctamente los callbacks de los proveedores. `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` es una **configuracion solo de desarrollo local**: no reutilizarla en un servidor publico sin establecer los proxies confiables y limitar el acceso a la API.

## Certificados HTTPS

Se reutiliza `.https/localhost.pfx`. Nginx no lee PFX directamente. Hay que convertir el certificado local a `.https/nginx-localhost.crt` y la clave a `.https/nginx-localhost.key` con OpenSSL. Los archivos quedan excluidos de Git.

```bash
openssl pkcs12 -in .https/localhost.pfx -clcerts -nokeys -out .https/nginx-localhost.crt
openssl pkcs12 -in .https/localhost.pfx -nocerts -nodes -out .https/nginx-localhost.key
chmod 600 .https/nginx-localhost.key
```

Los comandos preguntan por la contrasena del PFX. Nunca incluir la clave privada en la PR o en capturas.

## Inicio completo (un solo comando)

**No usar `oauth.local.yml` con este compose**: esa superposicion hace que la API intente ocupar `8443`, que ahora pertenece al frontend Nginx.

Primero detener la configuracion anterior **sin borrar volumenes**:

```bash
docker compose --env-file .env -f compose.dev.yml -f oauth.local.yml down
```

Luego iniciar los tres servicios:

```bash
docker compose --env-file .env -f compose.dev.yml up -d --build
```

Verificar:

```bash
docker compose --env-file .env -f compose.dev.yml ps
curl -k -i https://localhost:8443/
curl -k -i https://localhost:8443/health/db
curl -k -i https://localhost:8443/api/argentina/jugadores
```

Se esperan HTTP 200 en raiz y health/db, y HTTP 401 sin cookie en el endpoint protegido.

## Prueba de autenticacion

1. Abrir `https://localhost:8443/`.
2. Iniciar sesion con Twitch o Slack y comprobar que `/api/auth/me` devuelve HTTP 200.
3. Cerrar sesion desde el boton **Salir**.
4. Comprobar HTTP 401 en `/api/auth/me` y un endpoint protegido.
5. Los proveedores Google, Discord, Dropbox y Spotify requieren credenciales reales y callbacks registrados por sus responsables.

**Importante:** las PR de los reportes y de las mejoras del frontend deben integrarse a `main` para que el frontend Nginx final incluya esas pantallas.

## Evidencias minimas

1. `docker compose ps` muestra `frontend`, `api` y `postgres` (healthy).
2. `https://localhost:8443/` muestra la interfaz de Circle Rush, login y logout reales.
3. HTTP 401 tras cerrar sesion. Se pueden reutilizar capturas anteriores si esta version tambien lo verifica.

No publicar secretos, cookies ni certificados privados.
