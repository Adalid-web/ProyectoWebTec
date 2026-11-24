# Proyecto Final - Punto 7: Verificacion de cuatro instancias

## Responsable
Adalid Callizaya - Argentina

## Objetivo
Comprobar el funcionamiento simultaneo de cuatro instancias
del backend .NET 10 utilizando una imagen de Docker Hub.

## Imagen utilizada
adalidweb/circle-rush-api:70cf748

## Despliegue
Se utilizo compose.release.yml, preparado por el equipo.

Instancias:
- api1: puerto 5001
- api2: puerto 5002
- api3: puerto 5003
- api4: puerto 5004

Las cuatro instancias comparten PostgreSQL y el volumen
proyectotecweb-release_authkeys.

## Resultados de las pruebas

| Prueba | API 1 | API 2 | API 3 | API 4 |
|---|---|---|---|---|
| Health | 200 | 200 | 200 | 200 |
| PostgreSQL | 200 | 200 | 200 | 200 |
| Reporte sin autenticacion | 401 | 401 | 401 | 401 |

Se verifico que las cuatro instancias utilizan exactamente
la misma imagen publicada en Docker Hub.

## Claves compartidas

Las cuatro instancias montan el volumen:
proyectotecweb-release_authkeys

Se comprobo que las cuatro pueden acceder al mismo
archivo XML de Data Protection.

## Autenticacion compartida

Se realizo una prueba temporal utilizando Nginx como
balanceador de carga en https://localhost:8443.

Se inicio sesion de manera real mediante Twitch.

Desde Firefox se realizaron 16 peticiones al endpoint:
/api/auth/me

Resultados:
- 16 peticiones HTTP 200.
- Cuatro backends diferentes respondieron.
- Las cuatro instancias reconocieron la misma sesion.

Resultado final de la consola de Firefox:
Backends autenticados diferentes: 4

## Restauracion del entorno

Despues de las pruebas, se restauro la configuracion
original de Nginx y se desconecto la red temporal.

Verificaciones posteriores:
- Frontend: HTTP 200.
- PostgreSQL: HTTP 200.

## Observaciones de integracion

Para ejecutar desarrollo y release simultaneamente,
se utilizo un nombre independiente para PostgreSQL:
proyectotecweb-db-release

Los puertos de las APIs se limitaron a 127.0.0.1
durante las pruebas locales.

Estas correcciones deben contemplarse en el Compose
definitivo del equipo.

## Seguridad

No se incluyen credenciales OAuth, cookies,
claves privadas ni archivos .env.
