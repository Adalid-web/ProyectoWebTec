# Proyecto Final - Punto 2: Reporte autorizado de Argentina

**Autor:** Adalid Callizaya (Argentina). **Backend:** ASP.NET Core 10.

**Endpoint:** `GET /api/reportes/argentina/resumen`.

**Tablas:** `jugadores_argentina`, `partidas_argentina`, `logros_argentina`, `leaderboard_argentina`.

**Proposito:** mostrar cantidad de jugadores, partidas, logros, puntos acumulados y mejores puntajes, con un detalle de rendimiento por jugador. Las consultas usan los filtros EF Core de soft delete, por lo que no se contabilizan filas eliminadas. El ranking usa `leaderboard_argentina` y se muestra por separado porque el modelo actual no posee una FK hacia jugadores.

## Seguridad: permiso de backend, no solo boton

Se agrego `ReporteArgentina` a la configuracion de ASP.NET Core y se aplica con `[Authorize(Policy = "ReporteArgentina")]`.

- Sin sesion: **401 Unauthorized**.
- Con Twitch o Slack: **200 OK** y datos reales de las cuatro tablas.
- Con Google, Discord, Dropbox o Spotify: **403 Forbidden** (con sesion real de esos proveedores).

El control se hace con el claim `provider` que ya crea el login del proyecto.

## Ejecucion en Ubuntu

```bash
cd ~/Tarea6/ProyectoWebTec
git status --short
# En rama independiente y limpia, aplicar el parche del paquete
# git apply --check ../ProyectoFinal_Reporte_Argentina_Punto2.patch
# git apply ../ProyectoFinal_Reporte_Argentina_Punto2.patch
dotnet build ProyectoTecWeb/ProyectoTecWeb.csproj
docker compose --env-file .env -f contructor.yml -f oauth.local.yml up -d --build
```

Abrir `https://localhost:8443/`. Pulsar **Consultar reporte** desde Circle Rush. Si ya se inicio sesion con Twitch o Slack, se muestran los indicadores y las dos tablas.

## Evidencias esenciales (3 capturas)

1. `01_Reporte_Argentina_200.png`: frontend con el reporte visible y usuario Twitch o Slack.
2. `02_Reporte_Argentina_401.png`: Swagger con GET sin sesion (navegador privado).
3. `03_Reporte_Argentina_403.png`: Swagger con GET autenticado por un proveedor no permitido (p. ej. Discord o Google).

**Si solo tienes tus credenciales de Twitch y Slack**, puedes demostrar 200 y 401, pero para el **403 necesitas probar con una sesion real de proveedor no autorizado**, no inventar el resultado.

## Pull Request

Base `NardoLj/ProyectoWebTec:main` <- `Adalid-web/ProyectoWebTec:feat/final-reporte-argentina`.

**Titulo:** `feat(final): reporte autorizado de Argentina (punto 2)`

**Descripcion:** Reporte combinado de cuatro tablas, politica de backend `ReporteArgentina` (Twitch/Slack), panel funcional en Circle Rush. Pruebas: 200 autorizado, 401 sin sesion, 403 con proveedor distinto. Autor: Adalid Callizaya.

No mezclar en esta PR TikTok, Single Sign-Out, Dockerfiles ni Compose.
