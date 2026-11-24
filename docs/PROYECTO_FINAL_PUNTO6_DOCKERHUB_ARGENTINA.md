# Proyecto Final - Punto 6: Publicacion en Docker Hub

## Responsable
Adalid Callizaya - Argentina

## Objetivo
Publicar la imagen del backend ASP.NET Core .NET 10
en Docker Hub y comprobar su descarga.

## Repositorio publicado
https://hub.docker.com/r/adalidweb/circle-rush-api

## Imagen
adalidweb/circle-rush-api:70cf748

## Version del proyecto
Commit: 70cf748

## Construccion
La imagen se construyo desde el proyecto integrado
en la rama main del repositorio del equipo.

docker compose --env-file .env \
  -f compose.dev.yml \
  up -d --build

## Publicacion

docker tag circle-rush-api:dev \
  adalidweb/circle-rush-api:70cf748

docker push adalidweb/circle-rush-api:70cf748

## Descarga

docker pull adalidweb/circle-rush-api:70cf748

## Resultado
La publicacion y descarga finalizaron correctamente.

Digest:
sha256:b2f041b98f676b651acdf8f71c66d02260ad87ccd4463296609ca7e9fff52ac3

## Pruebas realizadas
- Frontend Nginx: HTTP 200.
- Conexion PostgreSQL: HTTP 200.
- API protegida sin autenticacion: HTTP 401.
- Docker push: exitoso.
- Docker pull: exitoso.

## Seguridad
No se incluyen contrasenas, tokens ni archivos .env.

## Nota
Esta es la imagen publicada por el integrante
responsable de Argentina. La imagen oficial del
equipo puede utilizar otra cuenta y etiqueta.
