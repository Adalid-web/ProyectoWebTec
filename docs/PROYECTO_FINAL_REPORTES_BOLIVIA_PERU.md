# Proyecto Final - Punto 2: reportes de Bolivia y Perú

Este documento completa el requisito de un reporte autorizado por integrante. El reporte de Argentina ya fue integrado en la PR #18.

## Reporte de Bolivia

- **Autor:** Leonardo Julio.
- **Propósito:** mostrar el rendimiento de los jugadores bolivianos, la cantidad de partidas y logros, los puntos acumulados y los mejores puntajes activos.
- **Endpoint:** `GET /api/reportes/bolivia/resumen`.
- **Tablas:** `jugadores_bolivia`, `partidas_bolivia`, `logros_bolivia`, `leaderboard_bolivia`.
- **Autorización:** política `ReporteBolivia`; solo sesiones cuyo proveedor sea Google o Discord.
- **Soft delete:** las consultas usan los filtros globales de EF Core, por lo que no se cuentan registros eliminados.

## Reporte de Perú

- **Autor:** Jheferson Velasco.
- **Propósito:** mostrar el rendimiento de los jugadores peruanos, la cantidad de partidas y logros, los puntos acumulados y los mejores puntajes.
- **Endpoint:** `GET /api/reportes/peru/resumen`.
- **Tablas:** `jugadores_peru`, `partidas_peru`, `logros_peru`, `leaderboard_peru`.
- **Autorización:** política `ReportePeru`; solo sesiones cuyo proveedor sea Dropbox o Spotify.

## Respuestas esperadas

| Situación | Resultado |
|---|---|
| Sin cookie de sesión | HTTP 401 |
| Sesión de un proveedor no autorizado | HTTP 403 |
| Sesión del proveedor autorizado | HTTP 200 con datos de las cuatro tablas |

## Frontend

El frontend incluye una tarjeta para cada reporte. El botón solo es una ayuda visual: la validación real se ejecuta en el backend con `[Authorize(Policy = ...)]`.

Para la demostración se deben crear algunos jugadores, partidas, logros y puntajes en cada país; de lo contrario el reporte responde correctamente pero con totales en cero.
