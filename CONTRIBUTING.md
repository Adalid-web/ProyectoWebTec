# Guía de contribución

Este repositorio se trabaja con **forks y Pull Requests**. Nadie debe subir cambios directamente a `main` y ningún integrante debe ser agregado como colaborador del repositorio base.

- Repositorio base: <https://github.com/NardoLj/ProyectoWebTec>
- Líder: **Leonardo Julio** (`NardoLj`)
- Integrantes: **Jheferson Velasco** y **Adalid Calizaya**

> Si un integrante aparece como colaborador del repositorio base, su nota será 0/100 según la consigna. Los aportes se hacen siempre desde el fork.

## 1. Preparar el fork

Cada integrante debe entrar al repositorio base y usar **Fork**. Luego, en su equipo:

```powershell
git clone https://github.com/<tu-usuario>/ProyectoWebTec.git
cd ProyectoWebTec

git remote add upstream https://github.com/NardoLj/ProyectoWebTec.git
git fetch upstream
git switch main
git merge upstream/main
```

Antes de empezar **cada etapa**, hay que sincronizar el fork con el repositorio base:

```powershell
git fetch upstream
git switch main
git merge upstream/main
git push origin main
```

El líder trabaja con ramas en el repositorio base y también abre Pull Requests. Sus PR deben ser revisadas por otro integrante; el líder no puede aprobar su propia PR.

## 2. Ramas

Usar nombres descriptivos, en minúsculas y separados por guiones:

| Etapa | Rama sugerida |
|---|---|
| Documentación | `docs/equipo` |
| Cuatro tablas | `feat/<pais>-tablas` |
| Dos logins | `feat/<pais>-auth` |
| Controladores POST/GET protegidos | `feat/<pais>-controllers` |
| PR fallida | `practice/<pais>-post-duplicado` |
| PUT + soft delete | `feat/<pais>-put-softdelete` |

Ejemplos:

- `feat/bolivia-tablas`
- `feat/peru-auth`
- `feat/argentina-controllers`
- `practice/argentina-post-duplicado`
- `feat/peru-put-softdelete`

Cada rama debe salir de la versión integrada de `main`, no de otra rama de trabajo. No mezclar dos etapas en una misma PR, salvo la etapa 5 que exige exactamente dos commits.

## 3. Commits

Usar mensajes claros y en español. Formato recomendado:

```text
feat(bolivia): agregar tablas de jugadores, partidas y logros
feat(bolivia): agregar login con Google y Discord
feat(bolivia): proteger controladores con Authorize
feat(bolivia): agregar PUT a los controladores
feat(bolivia): implementar soft delete
fix(auth): corregir callback de Discord
docs(equipo): documentar reparto de tablas y proveedores
```

Reglas:

- Un commit debe tener un objetivo claro.
- No incluir `.env`, contraseñas, tokens, cadenas de conexión reales ni capturas con secretos.
- No incluir `bin/` ni `obj/`.
- En la etapa 5 debe haber **dos commits separados**: uno para los PUT y otro para el soft delete. No hacer squash local antes de abrir la PR.

## 4. Probar antes de abrir la PR

Desde la raíz del repositorio:

```powershell
dotnet restore
dotnet build -c Release
```

Si se modificaron migraciones:

```powershell
cd ProyectoTecWeb
dotnet ef migrations list
dotnet ef database update
```

Si se modificó Docker:

```powershell
docker compose --env-file .env -f contructor.yml config
docker compose --env-file .env -f despliegue.yml config
```

Pruebas mínimas de la API:

1. Abrir `http://localhost:8080/swagger` o usar `curl.exe`.
2. Probar `POST`, `GET` de lista y `GET` por ID de las cuatro tablas.
3. Iniciar sesión con los dos proveedores asignados.
4. Verificar que sin sesión los endpoints protegidos respondan **401**.
5. En la etapa 5, probar `PUT`, `DELETE`, consulta posterior y confirmar en PostgreSQL que la fila sigue existiendo con `is_deleted = true`.

Adjuntar en la descripción de la PR:

- Qué se cambió.
- Cómo se probó.
- Capturas o salida de los comandos.
- Proveedores usados y tablas afectadas.

## 5. Abrir la Pull Request

1. Subir la rama al fork:

   ```powershell
   git push -u origin <nombre-de-la-rama>
   ```

2. En GitHub, abrir una PR desde `<tu-usuario>:<rama>` hacia `NardoLj:main`.
3. Escribir un título con el formato `feat(bolivia): ...`.
4. Completar la descripción y solicitar revisión al líder (`@NardoLj`).
5. No hacer merge de la propia PR.
6. Si hay comentarios de revisión, corregir en la misma rama y volver a subir.

El líder debe:

- Revisar el código y las pruebas.
- Aprobar o pedir cambios con un comentario claro.
- Integrar la PR.
- En sus propias PR, pedir la revisión a otro integrante y hacer la integración después.

## 6. PR fallida de práctica

Cada integrante debe abrir una PR que agregue un `POST` con **el mismo método HTTP y la misma ruta** que otro `POST` existente en uno de sus controladores.

Ejemplo:

- Ruta existente: `POST /api/bolivia/jugadores`.
- Ruta duplicada en la PR: otro método `[HttpPost]` en el mismo controlador con la misma ruta.

El líder debe:

1. Detectar la duplicación.
2. Explicar en la revisión por qué ASP.NET no puede elegir entre dos acciones con la misma ruta y método.
3. Cerrar la PR **sin integrarla**.
4. Conservar la PR y los comentarios como evidencia.

La rama de práctica no debe fusionarse con `main`.

## 7. Etapa 5: PUT + soft delete con Squash and merge

La rama debe contener exactamente dos commits:

1. `feat(<pais>): agregar PUT a los controladores`
2. `feat(<pais>): implementar soft delete`

El soft delete debe:

- Marcar el registro como inactivo/eliminado.
- Conservar la fila físicamente en PostgreSQL.
- Excluir los registros eliminados de los GET.
- Impedir actualizar o eliminar otra vez un registro inactivo.
- Agregar una migración de EF Core.

El líder revisa la PR y usa **Squash and merge**. No usar “Create a merge commit” ni “Rebase and merge” en esta etapa. En la evidencia deben verse los dos commits de la rama y el único commit resultante en `main`.

## 8. Migraciones y conflictos

- Cada integrante debe sincronizar `main` antes de crear su migración.
- Usar nombres de migración únicos: `AddBoliviaTables`, `AddPeruTables`, `AddArgentinaSoftDelete`, etc.
- No editar a mano `AppDbContextModelSnapshot.cs` salvo que sea imprescindible.
- Si hay conflicto en el snapshot, resolverlo regenerando la migración sobre la versión más reciente de `main`.

## 9. Secretos y archivos que no se suben

Nunca versionar:

- `.env` o `.env.local`.
- Client Secrets de Google, Discord, Microsoft, Twitch, Slack o Dropbox.
- Contraseñas de PostgreSQL.
- Capturas donde se vean secretos.
- `bin/`, `obj/` o archivos `.zip`.

El archivo `.env.example` sí se versiona y solo contiene valores de ejemplo.

## 10. Evidencias

Cada integrante debe conservar y entregar:

- Enlace al repositorio base.
- Enlace a su fork.
- Enlaces a sus PR de cada etapa.
- Captura de la aprobación/revisión.
- Captura del login con cada proveedor.
- Captura de la identidad de sesión y del logout.
- Pruebas de endpoints con y sin sesión.
- Captura de la PR fallida y su cierre.
- Captura de los dos commits y del squash final.

El documento final debe ser PDF o Word y el proyecto comprimido no debe incluir `.env`, `bin/` ni `obj/`.
