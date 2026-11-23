# Configuración de autenticación

La aplicación usa **cookie de sesión + OAuth**. No se usa JWT: el navegador recibe una cookie `HttpOnly` después del login y la API de mismo origen la envía automáticamente en cada petición.

## 1. Reparto de proveedores

Los proveedores no se repiten entre integrantes. Cada uno implementa uno de la lista fácil y otro de la lista media/difícil.

| Integrante | País | Proveedor fácil | Proveedor medio/difícil | Estado |
|---|---|---|---|---|
| Leonardo Julio | Bolivia | Discord | Google | En implementación |
| Jheferson Velasco | Perú | Twitch | Microsoft | Pendiente |
| Adalid Calizaya | Argentina | Dropbox | Slack | Pendiente |

## 2. Variables de entorno

Las credenciales reales se guardan únicamente en `.env` (ignorado por Git) o en `dotnet user-secrets`. Nunca se suben al repositorio, al PDF ni al ZIP.

### Para Docker Compose

Copiar `.env.example` a `.env` y completar:

```env
GOOGLE_CLIENT_ID=tu-client-id.apps.googleusercontent.com
GOOGLE_CLIENT_SECRET=tu-client-secret

DISCORD_CLIENT_ID=tu-discord-client-id
DISCORD_CLIENT_SECRET=tu-discord-client-secret

APP_PUBLIC_URL=http://localhost:8080
```

Los archivos `contructor.yml` y `despliegue.yml` convierten esas variables a la configuración de ASP.NET Core:

```yaml
Authentication__Google__ClientId: ${GOOGLE_CLIENT_ID}
Authentication__Google__ClientSecret: ${GOOGLE_CLIENT_SECRET}
Authentication__Discord__ClientId: ${DISCORD_CLIENT_ID}
Authentication__Discord__ClientSecret: ${DISCORD_CLIENT_SECRET}
```

### Para ejecutar con `dotnet run`

Opción A, variables de PowerShell:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:Authentication__Google__ClientId = "tu-client-id.apps.googleusercontent.com"
$env:Authentication__Google__ClientSecret = "tu-client-secret"
$env:Authentication__Discord__ClientId = "tu-discord-client-id"
$env:Authentication__Discord__ClientSecret = "tu-discord-client-secret"
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=proyectotecweb;Username=proyectotecweb;Password=tu-clave"
dotnet run --project .\ProyectoTecWeb
```

Opción B, user-secrets:

```powershell
cd .\ProyectoTecWeb
dotnet user-secrets init
dotnet user-secrets set "Authentication:Google:ClientId" "tu-client-id.apps.googleusercontent.com"
dotnet user-secrets set "Authentication:Google:ClientSecret" "tu-client-secret"
dotnet user-secrets set "Authentication:Discord:ClientId" "tu-discord-client-id"
dotnet user-secrets set "Authentication:Discord:ClientSecret" "tu-discord-client-secret"
```

La aplicación solo registra un proveedor si sus dos credenciales están presentes. Si no hay credenciales, arranca igual y el botón correspondiente no aparece.

## 3. Configurar Google

1. Entrar a <https://console.cloud.google.com/>.
2. Crear un proyecto, por ejemplo `ProyectoTecWeb`.
3. Ir a **APIs y servicios → Pantalla de consentimiento OAuth**.
4. Elegir **Externo**, completar nombre, correo de soporte y datos de contacto.
5. En **Usuarios de prueba**, agregar el correo con el que se probará el login.
6. Ir a **APIs y servicios → Credenciales → Crear credenciales → ID de cliente de OAuth**.
7. Tipo de aplicación: **Aplicación web**.
8. En **URIs de redirección autorizados**, agregar exactamente:

   ```text
   http://localhost:8080/signin-google
   ```

9. Guardar y copiar **Client ID** y **Client Secret** en `.env`.
10. El correo usado para probar debe estar agregado como usuario de prueba mientras la app esté en modo Testing.

### Errores frecuentes de Google

- `redirect_uri_mismatch`: la URI no coincide exactamente. Debe ser `http://localhost:8080/signin-google`, sin barra final adicional.
- `access_blocked`: la cuenta no está agregada como usuario de prueba.
- `invalid_client`: Client ID o Secret incorrectos, o `.env` no fue cargado.
- El correo no aparece: revisar el scope `email` y que la cuenta tenga correo verificado.

## 4. Configurar Discord

1. Entrar a <https://discord.com/developers/applications>.
2. Crear una aplicación nueva.
3. Ir a **OAuth2 → General**.
4. Copiar **Client ID** y **Client Secret**.
5. En **Redirects**, agregar exactamente:

   ```text
   http://localhost:8080/signin-discord
   ```

6. Guardar los cambios.
7. Scopes usados por la aplicación: `identify` y `email`.
8. Copiar las credenciales a `.env`.

### Errores frecuentes de Discord

- `invalid_redirect_uri`: la URL no está registrada exactamente en el portal.
- `invalid_client`: Client ID o Secret incorrectos.
- No se obtiene el correo: el usuario puede tener el correo no verificado; la aplicación debe manejar el valor nulo.

## 5. Callbacks y endpoints

| Proveedor | Botón | Callback |
|---|---|---|
| Google | `GET /login/google` | `http://localhost:8080/signin-google` |
| Discord | `GET /login/discord` | `http://localhost:8080/signin-discord` |

Endpoints de sesión:

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/auth/providers` | Lista los proveedores configurados |
| `GET` | `/api/auth/me` | Devuelve la identidad de la sesión; 401 sin sesión |
| `POST` | `/logout` | Cierra la sesión y borra la cookie |

El frontend consulta `/api/auth/providers` y muestra únicamente los botones disponibles. Después del callback, el navegador vuelve a `/` y el JavaScript consulta `/api/auth/me` para mostrar nombre, correo y proveedor.

## 6. Protección de la API

Los controladores de las cuatro tablas de cada integrante llevan `[Authorize]`. Sin cookie válida:

- `GET` de lista responde **401**.
- `GET` por ID responde **401**.
- `POST`, `PUT` y `DELETE` responden **401**.

Con sesión iniciada, el mismo endpoint responde normalmente. Los eventos de la cookie están configurados para devolver 401 en rutas `/api` en lugar de redirigir al login.

## 7. Cómo probar el flujo completo

1. Levantar PostgreSQL y la API con `docker compose --env-file .env -f contructor.yml up --build -d`.
2. Abrir `http://localhost:8080/`.
3. Comprobar que sin sesión el leaderboard muestra el mensaje de login y la API devuelve 401:

   ```powershell
   curl.exe -i http://localhost:8080/api/leaderboard/bolivia
   ```

4. Hacer clic en **Continuar con Google**, autenticarse y comprobar que vuelve a `/`.
5. Verificar la identidad mostrada en la cabecera.
6. Guardar una puntuación.
7. Cerrar sesión y comprobar que el endpoint vuelve a responder 401.
8. Repetir con **Discord**.
9. En Swagger o con `curl.exe`, probar GET y POST sin cookie para evidenciar el rechazo.

Para pruebas con cookie desde PowerShell se puede usar una sesión de navegador o `curl.exe -c cookies.txt -b cookies.txt`; para la evidencia es más claro usar el navegador y capturas de pantalla.

## 8. Seguridad

- No commitear `.env`.
- No mostrar secretos en capturas, README, PDF ni ZIP.
- Usar HTTPS en producción. `http://localhost` se acepta para desarrollo.
- La cookie es `HttpOnly`, `SameSite=Lax` y `SecurePolicy=SameAsRequest`.
- El logout es `POST` para evitar que un enlace externo cierre la sesión.
- Si se publica la app en un dominio, actualizar las redirect URI en Google y Discord y definir `APP_PUBLIC_URL` con la URL pública.

## 9. Variables para los demás proveedores

Cuando se integren los proveedores de los compañeros, agregar sus credenciales con el mismo patrón:

```env
MICROSOFT_CLIENT_ID=
MICROSOFT_CLIENT_SECRET=
TWITCH_CLIENT_ID=
TWITCH_CLIENT_SECRET=
SLACK_CLIENT_ID=
SLACK_CLIENT_SECRET=
DROPBOX_CLIENT_ID=
DROPBOX_CLIENT_SECRET=
```

Los callbacks propuestos son:

```text
http://localhost:8080/signin-microsoft
http://localhost:8080/signin-twitch
http://localhost:8080/signin-slack
http://localhost:8080/signin-dropbox
```

Cada compañero debe confirmar que el proveedor permite `http://localhost` en desarrollo. Si exige HTTPS, se puede usar un túnel de desarrollo y actualizar la redirect URI.
