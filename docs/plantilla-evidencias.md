# Plantilla de evidencias individuales

> Copiar este archivo a `docs/evidencias-<integrante>.md`, completar los datos y exportarlo a PDF o Word.
> No incluir contraseñas, Client Secrets, cookies ni el contenido de `.env` en el documento final.

## 1. Portada

| Dato | Respuesta |
|---|---|
| Nombre completo | `[Nombre]` |
| Equipo | `ProyectoTecWeb` |
| Rol | `[Líder / Integrante]` |
| País / servidor | `[Bolivia / Perú / Argentina]` |
| Repositorio base | <https://github.com/NardoLj/ProyectoWebTec> |
| Fork | `https://github.com/[usuario]/ProyectoWebTec` |
| Fecha | `[dd/mm/aaaa]` |

## 2. Declaración de alcance

- Mi parte corresponde a las cuatro tablas de `[país]`.
- Mis dos proveedores son `[proveedor fácil]` y `[proveedor medio/difícil]`.
- Trabajé desde mi fork y envié mis cambios mediante Pull Requests.
- No soy colaborador del repositorio base.
- No incluí credenciales reales en Git, capturas, documento ni ZIP.

## 3. Reparto del equipo

| Integrante | País | Cuatro tablas | Proveedor fácil | Proveedor medio/difícil |
|---|---|---|---|---|
| Leonardo Julio | Bolivia | `leaderboard_bolivia`, `jugadores_bolivia`, `partidas_bolivia`, `logros_bolivia` | Discord | Google |
| Jheferson Velasco | Perú | `leaderboard_peru`, `jugadores_peru`, `partidas_peru`, `logros_peru` | Twitch | Microsoft |
| Adalid Calizaya | Argentina | `leaderboard_argentina`, `jugadores_argentina`, `partidas_argentina`, `logros_argentina` | Dropbox | Slack |

## 4. Las cuatro tablas

| Tabla | Modelo | Configuración | Controlador | Ruta base |
|---|---|---|---|---|
| `[tabla_1]` | `[Modelo1]` | `[Modelo1Configuration]` | `[Controlador1]` | `/api/...` |
| `[tabla_2]` | `[Modelo2]` | `[Modelo2Configuration]` | `[Controlador2]` | `/api/...` |
| `[tabla_3]` | `[Modelo3]` | `[Modelo3Configuration]` | `[Controlador3]` | `/api/...` |
| `[tabla_4]` | `[Modelo4]` | `[Modelo4Configuration]` | `[Controlador4]` | `/api/...` |

Adjuntar:

- Captura del `DbContext` con los cuatro `DbSet`.
- Captura de la migración donde se crean las tablas.
- Captura de `\dt` en PostgreSQL con las cuatro tablas.
- Enlace al commit o PR de las tablas.

## 5. Los dos logins

| Proveedor | Tipo | Botón | Callback | Captura de login | Captura de identidad |
|---|---|---|---|---|---|
| `[Google/Discord/...]` | Fácil | `/login/...` | `http://localhost:8080/signin-...` | `[Figura]` | `[Figura]` |
| `[Microsoft/Twitch/...]` | Medio | `/login/...` | `http://localhost:8080/signin-...` | `[Figura]` | `[Figura]` |

Incluir:

- Captura de la pantalla de consentimiento de cada proveedor.
- Captura de la página principal con nombre, correo y proveedor.
- Captura o salida de `GET /api/auth/me` con la identidad.
- Captura del botón **Salir** y de `GET /api/auth/me` respondiendo 401 después del logout.
- Explicación breve de dónde se configuraron las credenciales (solo nombres de variables, nunca valores).
- Captura de la aplicación de Google/Discord con la redirect URI registrada, ocultando el Client Secret.

## 6. API protegida

Completar la tabla con las pruebas reales:

| Tabla | POST sin sesión | POST con sesión | GET lista con sesión | GET por ID con sesión |
|---|---|---|---|---|
| `[tabla_1]` | 401 | 201 | 200 | 200 |
| `[tabla_2]` | 401 | 201 | 200 | 200 |
| `[tabla_3]` | 401 | 201 | 200 | 200 |
| `[tabla_4]` | 401 | 201 | 200 | 200 |

Adjuntar capturas de Swagger, `curl.exe` o PowerShell. Para la evidencia de rechazo se puede usar:

```powershell
curl.exe -i http://localhost:8080/api/leaderboard/bolivia
curl.exe -i http://localhost:8080/api/bolivia/jugadores
```

El resultado esperado sin cookie es `HTTP/1.1 401 Unauthorized`.

## 7. PR fallida de práctica

| Dato | Respuesta |
|---|---|
| Número de PR | `#[número]` |
| Rama | `practice/[pais]-post-duplicado` |
| Ruta duplicada | `POST /api/...` |
| Estado | Cerrada sin merge |
| Enlace | `[URL de la PR]` |

Incluir:

- Captura de la PR con el método `[HttpPost]` duplicado.
- Captura del comentario del líder explicando que ASP.NET no puede distinguir dos acciones con el mismo verbo y la misma ruta.
- Captura del cierre sin merge.
- Explicación breve: el ruteo produce ambigüedad; se debe conservar una sola acción por combinación método + ruta.

## 8. PUT y soft delete

| Dato | Respuesta |
|---|---|
| PR | `#[número]` |
| Rama | `feat/[pais]-put-softdelete` |
| Commit 1 | `[hash]` — agregar PUT |
| Commit 2 | `[hash]` — implementar soft delete |
| Integración | Squash and merge |
| Commit resultante en `main` | `[hash]` |

Adjuntar:

- Captura de los dos commits en la pestaña **Commits** de la PR.
- Captura del botón/resultado **Squash and merge**.
- Captura del historial de `main` con un único commit de la etapa.
- Captura de `PUT` respondiendo 204.
- Captura de `DELETE` respondiendo 204.
- Captura de `GET` por ID respondiendo 404 después del DELETE.
- Captura de `PUT` y `DELETE` sobre el registro inactivo respondiendo 404.
- Captura de PostgreSQL mostrando la fila con `is_deleted = true` y `deleted_at` no nulo.

Consulta sugerida:

```sql
SELECT id, is_deleted, deleted_at
FROM [tabla]
ORDER BY id DESC
LIMIT 5;
```

## 9. Pull Requests y ramas

| Etapa | Rama | PR | Estado | Enlace |
|---|---|---|---|---|
| Tablas | `feat/[pais]-tablas` | `#[n]` | `[Aprobada/Merge]` | `[URL]` |
| Auth | `feat/[pais]-auth` | `#[n]` | `[Aprobada/Merge]` | `[URL]` |
| Controladores | `feat/[pais]-controllers` | `#[n]` | `[Aprobada/Merge]` | `[URL]` |
| PR fallida | `practice/[pais]-post-duplicado` | `#[n]` | Cerrada | `[URL]` |
| PUT + soft delete | `feat/[pais]-put-softdelete` | `#[n]` | Squash and merge | `[URL]` |

## 10. Ejecución

```powershell
Copy-Item .env.example .env
# Completar la contraseña de PostgreSQL y las credenciales OAuth en .env.

docker compose --env-file .env -f contructor.yml up --build -d
docker compose --env-file .env -f contructor.yml ps
```

Abrir:

- Juego: <http://localhost:8080/>
- Swagger: <http://localhost:8080/swagger>
- Health: <http://localhost:8080/health>

## 11. Checklist final

- [ ] Cuatro tablas con modelos, configuraciones, `DbSet` y migración.
- [ ] Dos proveedores distintos, uno fácil y uno medio/difícil.
- [ ] Credenciales solo por variables de entorno.
- [ ] Login con ambos proveedores probado.
- [ ] Identidad de sesión visible.
- [ ] Logout probado.
- [ ] POST, GET lista y GET por ID de las cuatro tablas.
- [ ] Endpoints sin sesión responden 401.
- [ ] PR con POST duplicado cerrada sin merge.
- [ ] PR de PUT + soft delete con dos commits.
- [ ] Integración con Squash and merge.
- [ ] Filas eliminadas conservadas en PostgreSQL.
- [ ] GET excluye inactivos.
- [ ] PUT/DELETE sobre inactivos responde 404.
- [ ] Enlaces a repo base, fork y PR.
- [ ] PDF/Word individual generado.
- [ ] ZIP sin `.env`, `bin` ni `obj`.
- [ ] Sin secretos en capturas ni documentos.

## 12. Anexos

- `[Captura adicional]`
- `[Salida de comandos]`
- `[Enlace a video o carpeta de capturas]`
