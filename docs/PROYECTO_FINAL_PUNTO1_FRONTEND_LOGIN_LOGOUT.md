# Proyecto Final · Punto 1 — Frontend, OAuth y logout (10 pts)

**Equipo:** Leonardo Julio (Bolivia), Jheferson Velasco (Perú), Adalid Callizaya (Argentina).

## Objetivo

Conservar el juego Circle Rush y mostrar un botón para cada proveedor implementado:
Discord, Google, Dropbox, Spotify, Twitch y Slack. Si faltan credenciales,
el botón permanece visible pero deshabilitado con un aviso. **Solo un proveedor
configurado y autorizado por su plataforma puede completar el login real.**

El botón `Salir` envía `POST /logout` con la cookie y verifica que
`GET /api/auth/me` devuelva `401` antes de actualizar el frontend.
Los recursos protegidos se validan **en el servidor** con `[Authorize]`.

## Archivos modificados

- `ProyectoTecWeb/Auth/AuthProviderCatalog.cs` y `AuthProviderInfo.cs`:
  devuelve seis proveedores y su atributo público `configured` (sin secretos).
- `ProyectoTecWeb/Auth/AuthEndpoints.cs`: la lista `/api/auth/providers`
  ahora devuelve los seis proveedores registrados.
- `ProyectoTecWeb/wwwroot/js/auth.js`: botones, identidad, logout verificable.
- `ProyectoTecWeb/wwwroot/css/site.css`: estilo de proveedores deshabilitados y avisos.
- `contructor.yml`: incluye las variables OAuth de Dropbox y Spotify.
- `.env.example`: plantilla actualizada, sin credenciales reales.

## Antes de probar

1. Contar con `.env` privado y un certificado HTTPS válido para el hostname usado.
2. Registrar las URLs de retorno exactas en cada portal OAuth:
   `https://localhost:8443/signin-google`, `/signin-discord`,
   `/signin-dropbox`, `/signin-spotify`, `/signin-twitch`, `/signin-slack`,
   **siempre que el proveedor permita ese origen**. Si no admite `localhost`,
   configurar un dominio HTTPS real y sus callbacks.
3. Cada integrante configura sus propias credenciales. No publicar secretos,
   cookies ni tokens en GitHub, capturas, PR o informe.

## Comandos de prueba (Ubuntu)

```bash
cd ~/ProyectoFinal/ProyectoWebTec

dotnet build ProyectoTecWeb/ProyectoTecWeb.csproj

docker compose --env-file .env -f contructor.yml -f oauth.local.yml up -d --build

docker compose --env-file .env -f contructor.yml -f oauth.local.yml ps

curl -k -s https://localhost:8443/api/auth/providers | python3 -m json.tool
curl -k -i https://localhost:8443/api/auth/me
curl -k -i https://localhost:8443/api/leaderboard/argentina
```

La lista de proveedores debe contener seis entradas. `configured: true` significa
que hay credenciales *presentes*; **no demuestra que el proveedor autorice el login**.
Las dos consultas sin cookie deben devolver `401`.

## Matriz de verificación en Firefox (https://localhost:8443/)

| Paso | Resultado esperado |
|---|---|
| Sin sesión | Seis botones visibles; sin credenciales se deshabilitan |
| Login Discord | Redirección real, regreso e identidad `provider: Discord` |
| Login Google | Redirección real, regreso e identidad `provider: Google` |
| Login Dropbox | Redirección real, regreso e identidad `provider: Dropbox` |
| Login Spotify | Redirección real, regreso e identidad `provider: Spotify` |
| Login Twitch | Redirección real, regreso e identidad `provider: Twitch` |
| Login Slack | Redirección real, regreso e identidad `provider: Slack` |
| Logout desde cada sesión | Botón «Salir», verificación de `401` para `/api/auth/me` |
| Tras salir | Endpoint `/api/leaderboard/argentina` devuelve `401` |

El logout es de **Circle Rush**. No cierra la sesión global en Discord,
Google, Dropbox, Spotify, Twitch o Slack (no se pide single sign-out en el punto 1).

## Evidencias mínimas para la entrega

1. Navegador mostrando los seis botones (pueden estar deshabilitados si faltan credenciales).
2. Captura conjunta de `/api/auth/me` con Twitch y Slack, y las pruebas equivalentes
   de Bolivia y Perú recopiladas de los responsables.
3. Captura del frontend con identidad y cierre de sesión; F12 → Red:
   `/api/auth/me` devuelve HTTP 401 y `/api/leaderboard/argentina` rechaza sin sesión.
4. `docker compose ps` (se reutiliza también como evidencia de otros puntos).

## Limitaciones pendientes de aprobación del equipo

- No se incluyen credenciales de terceros ni se puede garantizar que los seis
  logins funcionen sin completar la configuración real de cada plataforma.
- En el Proyecto Final se separará el frontend a un contenedor Nginx (punto 5).
  Este punto 1 conserva el frontend que sirve ASP.NET Core, para evitar romper
  los callbacks mientras se desarrolla el Compose independiente.
