# Proyecto Final - Punto 5: servidor independiente para Circle Rush.
# El contexto es la raiz del proyecto; .dockerignore excluye secretos.
FROM nginx:stable-alpine
COPY frontend/nginx.conf /etc/nginx/conf.d/default.conf
COPY ProyectoTecWeb/wwwroot/ /usr/share/nginx/html/
EXPOSE 443
