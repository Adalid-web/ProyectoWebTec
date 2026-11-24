# TikTok Sandbox – Circle Rush

Parche incremental. No reemplazar .env, .https, .auth-keys ni los volúmenes de PostgreSQL.

En .env, agregar una sola vez:

```dotenv
TIKTOK_CLIENT_KEY=
TIKTOK_CLIENT_SECRET=
TIKTOK_REDIRECT_URI=https://TU-DOMINIO/signin-tiktok
```

Usar las credenciales del Sandbox, no Production. Revisar los términos y privacidad antes de exponerlos. No usar datos reales. Iniciar sesión desde el dominio HTTPS de ngrok, no desde localhost. Si ngrok presenta su aviso gratuito, continuar en el navegador antes de pulsar TikTok. No iniciar otros proveedores desde ese dominio si no tienen ese callback registrado.

```bash
docker compose --env-file .env -f compose.dev.yml up -d --build api frontend
curl -ks https://localhost:8443/api/auth/providers
```

TikTok debe aparecer con configured=true; no se muestran secretos. Probar una sola autorización con la cuenta Target Users. Si falla, no repetir sin revisar la configuración. Cerrar sesión y verificar /api/auth/me = 401. Para Single Sign-Out, iniciar sesión con TikTok en dos perfiles independientes del navegador, pulsar Salir de todos en uno y verificar que el otro responda 401 al actualizar. El cierre global es por proveedor e identidad, no revoca el acceso externo ni cierra tiktok.com. Las políticas de reportes originales no se modifican.

```bash
docker compose --env-file .env -f compose.dev.yml exec -T postgres sh -c 'psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" -c "SELECT provider, count(*) AS sesiones, count(*) FILTER (WHERE revoked_at IS NOT NULL) AS revocadas FROM circle_rush_sso_sessions GROUP BY provider;"'
```

Las imágenes release deben reconstruirse y publicarse antes de que el compose release pueda usar el nuevo código. No se realizó una prueba OAuth real ni compilación .NET en el entorno de preparación. Referencias: https://developers.tiktok.com/docs/en/login-kit-web ; https://developers.tiktok.com/docs/en/oauth-get-a-user-access-token ; https://developers.tiktok.com/docs/en/tiktok-api-v2-get-user-info/ .
