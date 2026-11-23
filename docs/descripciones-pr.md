# Descripciones listas para las Pull Requests

> Abrir las PR en el orden indicado. Cada PR debe salir de `main` ya actualizado con la PR anterior.
> En las etapas 1 a 4 usar **Create a merge commit**. En la etapa 5 usar **Squash and merge**.
> Las PR del líder deben ser revisadas por otro integrante, no por el propio líder.

## Orden de integración

| Orden | Rama | Base | Método | Estado esperado |
|---|---|---|---|---|
| 1 | `docs/equipo` | `main` | Create a merge commit | Merge |
| 2 | `feat/bolivia-tablas` | `main` | Create a merge commit | Merge |
| 3 | `feat/bolivia-auth` | `main` | Create a merge commit | Merge |
| 4 | `feat/bolivia-controllers` | `main` | Create a merge commit | Merge |
| 5 | `practice/bolivia-post-duplicado` | `main` | No merge | Cerrar |
| 6 | `feat/bolivia-put-softdelete` | `main` | **Squash and merge** | Merge |
| 7 | `docs/evidencias` | `main` | Create a merge commit | Merge |

Antes de abrir cada PR, sincronizar `main` local y esperar a que la PR anterior esté integrada. Si GitHub muestra commits de etapas anteriores en la pestaña **Files changed**, la PR anterior todavía no está en `main` o se integró con squash; en ese caso, actualizar la rama con `git rebase origin/main`.

---

## PR 1 · Documentación y reparto

**Título:**

```text
docs(equipo): agregar guía de contribución y reparto de tablas y proveedores
```

**Descripción:**

```markdown
## Qué cambia
- Agrega `CONTRIBUTING.md` con el flujo de forks, ramas, commits, pruebas, PR y revisión.
- Actualiza `README.md` con el equipo, las cuatro tablas por integrante y el reparto de proveedores.
- Agrega `docs/autenticacion.md` con la configuración de Google, Discord y proveedores futuros.
- Actualiza `.env.example` con placeholders, sin secretos.

## Reparto
| Integrante | País | Proveedores |
|---|---|---|
| Leonardo Julio | Bolivia | Discord + Google |
| Jheferson Velasco | Perú | Twitch + Microsoft |
| Adalid Calizaya | Argentina | Dropbox + Slack |

## Verificación
- [x] No se agregaron credenciales reales.
- [x] `.env` sigue ignorado.
- [x] El reparto no repite proveedores.
- [x] Se documenta que nadie debe ser colaborador del repositorio base.

## Revisión
Solicito revisión a un integrante del equipo para validar el reparto y el flujo de trabajo.
```

**Revisión esperada:** que otro integrante confirme que su país, sus tablas y sus proveedores están correctos.

---

## PR 2 · Tablas de Bolivia

**Título:**

```text
feat(bolivia): agregar tablas de jugadores, partidas y logros
```

**Descripción:**

```markdown
## Qué cambia
- Agrega los modelos `JugadorBolivia`, `PartidaBolivia` y `LogroBolivia`.
- Agrega sus configuraciones de EF Core y los `DbSet` en `AppDbContext`.
- Agrega la migración `AddBoliviaTables`.
- La cuarta tabla de Bolivia es `leaderboard_bolivia`, que ya existía.

## Tablas
- `jugadores_bolivia`
- `partidas_bolivia`
- `logros_bolivia`
- `leaderboard_bolivia`

## Verificación
- [x] `dotnet build -c Release`: 0 warnings, 0 errores.
- [x] `dotnet ef migrations list` incluye `AddBoliviaTables`.
- [x] La migración crea las tres tablas nuevas.
- [x] La relación jugador-partida y jugador-logro usa `DeleteBehavior.Restrict`.

## Revisión
Solicito revisar nombres de tablas, columnas, relaciones e índices.
```

**Revisión esperada:** que otro integrante revise que las cuatro tablas queden claramente asignadas a Bolivia y que la migración no toque las tablas de Argentina.

---

## PR 3 · Autenticación con Google y Discord

**Título:**

```text
feat(bolivia): agregar login con Google y Discord y sesión por cookie
```

**Descripción:**

```markdown
## Qué cambia
- Agrega autenticación por cookie y OAuth con Google y Discord.
- Agrega `/api/auth/providers`, `/api/auth/me`, `/login/google`, `/login/discord` y `POST /logout`.
- Agrega botones de login, identidad de sesión y logout en el frontend.
- Protege las llamadas del juego: sin sesión, la API responde 401.
- Agrega las credenciales por variables de entorno en `contructor.yml` y `despliegue.yml`.

## Configuración
Las credenciales reales no están en el repositorio. Se leen de:
- `GOOGLE_CLIENT_ID`
- `GOOGLE_CLIENT_SECRET`
- `DISCORD_CLIENT_ID`
- `DISCORD_CLIENT_SECRET`

Callbacks:
- `http://localhost:8080/signin-google`
- `http://localhost:8080/signin-discord`

## Verificación
- [x] `dotnet build -c Release`: 0 warnings, 0 errores.
- [x] La aplicación arranca sin credenciales y muestra solo los proveedores configurados.
- [x] `GET /api/auth/me` sin sesión responde 401.
- [x] `/login/google` sin configurar responde 503 con un mensaje claro.
- [x] Con credenciales, el login vuelve a `/` y muestra nombre, correo y proveedor.
- [x] `POST /logout` elimina la cookie.

## Revisión
Solicito revisar el flujo OAuth, el manejo de la cookie y que no haya secretos versionados.
```

**Antes de pedir revisión:** cargar las credenciales en `.env`, probar ambos logins y adjuntar capturas de identidad y logout. No mostrar los secretos.

---

## PR 4 · Controladores protegidos

**Título:**

```text
feat(bolivia): agregar controladores POST y GET protegidos
```

**Descripción:**

```markdown
## Qué cambia
- Agrega `JugadoresBoliviaController`, `PartidasBoliviaController` y `LogrosBoliviaController`.
- Agrega `[Authorize]` a los cuatro controladores de Bolivia.
- Implementa `POST`, `GET` de lista y `GET` por ID.
- Agrega DTO de entrada/salida con validaciones.
- Agrega validación de jugador existente para partidas y logros.

## Rutas
- `GET/POST /api/leaderboard/bolivia`
- `GET/POST /api/bolivia/jugadores`
- `GET/POST /api/bolivia/partidas`
- `GET/POST /api/bolivia/logros`

## Verificación
- [x] Sin cookie, los cuatro endpoints responden 401.
- [x] Con sesión, `POST` responde 201 y `GET` responde 200.
- [x] `GET` por ID inexistente responde 404.
- [x] Un alias duplicado responde 409.
- [x] Una partida o logro con jugador inexistente responde 404.

## Revisión
Solicito revisar el atributo `[Authorize]`, las rutas y las validaciones.
```

**Revisión esperada:** capturas de 401 sin sesión y de 201/200 con sesión para las cuatro tablas.

---

## PR 5 · PR fallida de práctica

**Título:**

```text
practice(bolivia): agregar POST duplicado para PR fallida
```

**Descripción:**

```markdown
## Objetivo
Esta PR es intencionalmente incorrecta. Agrega un segundo método `[HttpPost]` en `JugadoresBoliviaController` con la misma ruta que el `POST` existente.

## Problema
La ruta `POST /api/bolivia/jugadores` queda asociada a dos acciones. ASP.NET Core no puede elegir una acción de forma determinista y el ruteo falla por ambigüedad.

## Decisión esperada
El líder debe explicar el conflicto, cerrar la PR sin integrarla y conservar la evidencia.
```

**No integrar.** El líder debe dejar este comentario y cerrar:

```text
Revisado. La PR no se puede integrar porque agrega un segundo método [HttpPost] con la misma ruta que el POST existente: POST /api/bolivia/jugadores.

ASP.NET Core necesita una combinación única de método HTTP + ruta para seleccionar la acción. Al existir dos candidatos con la misma combinación, el ruteo queda ambiguo y no puede determinar cuál ejecutar. La corrección sería eliminar el método duplicado o darle una ruta distinta y justificarla.

Se cierra sin merge para conservar la evidencia de la práctica. La rama practice/bolivia-post-duplicado no se integra a main.
```

**Evidencia:** captura de la PR, del código duplicado, del comentario y del cierre sin merge.

---

## PR 6 · PUT y soft delete

**Título:**

```text
feat(bolivia): agregar PUT y soft delete
```

**Descripción:**

```markdown
## Qué cambia
La rama contiene exactamente dos commits:
1. `feat(bolivia): agregar PUT a los controladores`
2. `feat(bolivia): implementar soft delete en las cuatro tablas`

### PUT
- Agrega `PUT` a jugadores, partidas y logros.
- Mantiene el `PUT` existente del leaderboard.
- Valida jugador existente, alias único y campos obligatorios.

### Soft delete
- Agrega `is_deleted` y `deleted_at` a las cuatro tablas.
- Agrega filtros globales para excluir inactivos en los GET.
- `DELETE` marca la fila; no la borra físicamente.
- Un registro inactivo responde 404 en GET, PUT y DELETE.
- Agrega la migración `AddBoliviaSoftDelete`.

## Verificación
- [x] `PUT` responde 204 y el GET refleja el cambio.
- [x] `DELETE` responde 204.
- [x] `GET` por ID posterior responde 404.
- [x] `PUT` sobre inactivo responde 404.
- [x] `DELETE` sobre inactivo responde 404.
- [x] PostgreSQL conserva las filas con `is_deleted = true` y `deleted_at` no nulo.
- [x] `dotnet build -c Release`: 0 warnings, 0 errores.

## Integración
Solicito revisión y, si está conforme, usar **Squash and merge**.
```

**Evidencia obligatoria:** captura de los dos commits en la PR, del squash, de la fila conservada en PostgreSQL y de los 404.

---

## PR 7 · Plantilla de evidencias y script de entrega

**Título:**

```text
docs(evidencias): agregar plantilla y script de entrega
```

**Descripción:**

```markdown
## Qué cambia
- Agrega `docs/plantilla-evidencias.md` con la estructura del PDF/Word individual.
- Actualiza `scripts/test-crud.ps1` para probar 401 sin cookie y CRUD con cookie de sesión.
- Agrega `scripts/crear-entrega.ps1` para generar el ZIP sin `.env`, `bin`, `obj` ni `.git`.

## Verificación
- [x] El script de pruebas valida la sintaxis de PowerShell.
- [x] El ZIP de prueba no contiene `.env`, `bin`, `obj` ni `.git`.
- [x] La plantilla cubre tablas, dos proveedores, PR fallida, PUT, soft delete y squash.

## Revisión
Solicito revisar la plantilla y ejecutar el script de entrega.
```

---

## Revisión de las PR del líder

Como el líder no puede aprobar sus propias PR, pedir a Jheferson o Adalid que deje un comentario de revisión. Un comentario útil debe incluir:

```text
Revisé la rama y el diff. Confirmo que:
- [ ] Se cumplen los requisitos de esta etapa.
- [ ] No hay secretos ni .env versionado.
- [ ] La compilación pasa.
- [ ] Las pruebas indicadas en la descripción fueron realizadas.
- [ ] No se modifican tablas o controladores de otro integrante sin justificación.

Apruebo la PR.
```

Guardar captura de la revisión y de la aprobación para el PDF/Word.
