$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$outputs = Join-Path $PSScriptRoot 'outputs'
$htmlPath = Join-Path $PSScriptRoot 'evidencias.html'

function Get-OutputText([string]$name) {
    return (Get-Content -Raw -Encoding UTF8 (Join-Path $outputs $name)).TrimEnd()
}

function Get-FileText([string]$relativePath) {
    return (Get-Content -Raw -Encoding UTF8 (Join-Path $root $relativePath)).TrimEnd()
}

function HtmlCode([string]$text) {
    $encoded = [System.Net.WebUtility]::HtmlEncode($text)
    return "<pre>$encoded</pre>"
}

function OutputCode([string]$name) {
    return HtmlCode (Get-OutputText $name)
}

function FileCode([string]$relativePath) {
    return HtmlCode (Get-FileText $relativePath)
}

$dotnetVersion = Get-OutputText '00-dotnet-version.txt'
$dockerVersion = Get-OutputText '06-docker-version.txt'
$composeVersion = Get-OutputText '07-compose-version.txt'
$tagInfo = Get-Content -Raw -Encoding UTF8 (Join-Path $outputs '13-dockerhub-tag.json') | ConvertFrom-Json
$digest = $tagInfo.images[0].digest
$pushedAt = $tagInfo.images[0].last_pushed
$compressedMb = [math]::Round($tagInfo.full_size / 1MB, 1)

$html = @'
<!doctype html>
<html lang="es">
<head>
<meta charset="utf-8">
<title>Evidencias ProyectoTecWeb - Leonardo Julio</title>
<style>
  @page { size: A4; margin: 15mm 14mm 16mm 14mm; }
  * { box-sizing: border-box; }
  html { -webkit-print-color-adjust: exact; print-color-adjust: exact; }
  body {
    margin: 0;
    font-family: "Segoe UI", Arial, sans-serif;
    color: #1f2937;
    font-size: 10pt;
    line-height: 1.45;
    background: #ffffff;
  }
  h1, h2, h3 { color: #0f172a; margin: 0 0 6px 0; line-height: 1.2; }
  h1 { font-size: 22pt; }
  h2 { font-size: 16pt; border-bottom: 2px solid #0891b2; padding-bottom: 4px; margin-top: 0; }
  h3 { font-size: 12pt; margin-top: 14px; }
  p { margin: 6px 0; }
  ul, ol { margin: 6px 0 8px 20px; padding: 0; }
  li { margin: 3px 0; }
  small { color: #475569; }
  code {
    font-family: Consolas, "Cascadia Mono", monospace;
    font-size: 8.6pt;
    background: #e2e8f0;
    color: #0f172a;
    padding: 1px 4px;
    border-radius: 3px;
  }
  pre {
    font-family: Consolas, "Cascadia Mono", monospace;
    font-size: 7.9pt;
    line-height: 1.35;
    background: #f1f5f9;
    color: #0f172a;
    border: 1px solid #cbd5e1;
    border-left: 4px solid #0891b2;
    border-radius: 4px;
    padding: 8px 10px;
    white-space: pre-wrap;
    word-break: break-word;
    page-break-inside: auto;
    margin: 8px 0 12px 0;
  }
  table {
    width: 100%;
    border-collapse: collapse;
    margin: 8px 0 12px 0;
    font-size: 9pt;
    page-break-inside: avoid;
  }
  th, td { border: 1px solid #cbd5e1; padding: 5px 7px; vertical-align: top; text-align: left; }
  th { background: #e0f2fe; color: #0f172a; }
  tr:nth-child(even) td { background: #f8fafc; }
  .cover { page-break-after: always; padding-top: 18mm; }
  .cover .accent { height: 8px; width: 70mm; background: #0891b2; border-radius: 4px; margin-bottom: 16mm; }
  .cover .kicker { color: #0e7490; font-size: 12pt; font-weight: 600; letter-spacing: .5px; text-transform: uppercase; }
  .cover h1 { font-size: 34pt; margin: 6px 0 4px 0; }
  .cover .subtitle { font-size: 17pt; color: #334155; margin-bottom: 18mm; }
  .cover .meta { border-top: 1px solid #cbd5e1; padding-top: 8px; width: 100%; }
  .cover .meta td { border: none; padding: 4px 0; background: transparent; font-size: 10pt; }
  .cover .meta td:first-child { color: #64748b; width: 34mm; }
  .cover .stamp { margin-top: 20mm; padding: 10px 14px; border: 1px solid #0891b2; border-radius: 6px; background: #ecfeff; color: #155e75; font-size: 9.5pt; }
  .section { page-break-before: always; }
  .section-first { page-break-before: auto; }
  .lead { font-size: 11pt; color: #334155; }
  .note { background: #ecfeff; border: 1px solid #67e8f9; border-left: 4px solid #0891b2; border-radius: 4px; padding: 8px 10px; margin: 9px 0 12px 0; }
  .warn { background: #fff7ed; border: 1px solid #fdba74; border-left: 4px solid #f97316; border-radius: 4px; padding: 8px 10px; margin: 9px 0 12px 0; }
  .ok { color: #047857; font-weight: 600; }
  .fig { margin: 10px 0 14px 0; page-break-inside: avoid; }
  .fig img { width: auto; max-width: 100%; max-height: 205mm; height: auto; border: 1px solid #94a3b8; border-radius: 6px; display: block; margin: 0 auto; }
  .fig.small img { max-height: 165mm; }
  .fig figcaption { font-size: 8.5pt; color: #475569; margin-top: 4px; }
  .fig-grid { display: flex; gap: 10px; page-break-inside: avoid; }
  .fig-grid .fig { width: 50%; }
  .arch { display: flex; align-items: stretch; gap: 6px; margin: 10px 0 14px 0; page-break-inside: avoid; }
  .arch .box { flex: 1; border: 1px solid #94a3b8; border-radius: 6px; padding: 8px 6px; text-align: center; background: #f8fafc; font-size: 8.5pt; }
  .arch .box strong { display: block; font-size: 9.2pt; color: #0f172a; }
  .arch .arrow { align-self: center; color: #0891b2; font-size: 16pt; font-weight: 700; }
  .toc { page-break-after: always; }
  .toc ol { margin-top: 12px; }
  .toc li { margin: 6px 0; font-size: 10.5pt; }
  .two-col { display: flex; gap: 14px; }
  .two-col > div { width: 50%; }
  .small { font-size: 8.5pt; color: #475569; }
  .tag { display: inline-block; font-family: Consolas, monospace; font-size: 8pt; background: #e0f2fe; border: 1px solid #7dd3fc; color: #075985; border-radius: 4px; padding: 1px 5px; }
  .checklist td:last-child { white-space: nowrap; }
</style>
</head>
<body>

<section class="cover">
  <div class="accent"></div>
  <div class="kicker">Proyecto de Tecnología Web</div>
  <h1>ProyectoTecWeb</h1>
  <div class="subtitle">Circle Rush · Leaderboard Bolivia</div>
  <p class="lead">Documento de evidencias de implementación, pruebas y despliegue.</p>
  <table class="meta">
    <tr><td>Integrante</td><td><strong>Leonardo Julio</strong> — servidor Bolivia</td></tr>
    <tr><td>Tabla</td><td><code>leaderboard_bolivia</code> · <code>LeaderboardBoliviaController</code></td></tr>
    <tr><td>Repositorio</td><td>https://github.com/NardoLj/ProyectoWebTec</td></tr>
    <tr><td>Docker Hub</td><td>https://hub.docker.com/r/nardolj/proyectowebtec</td></tr>
    <tr><td>Imagen</td><td><code>nardolj/proyectowebtec:1.0.0</code> · digest <code>%%DIGEST%%</code></td></tr>
    <tr><td>Fecha</td><td>15 de noviembre de 2026</td></tr>
  </table>
  <div class="stamp">
    <strong>Entrega individual.</strong> Este documento cubre la parte de Leonardo Julio (Bolivia).
    Los servidores de Perú y Argentina están asignados a Jheferson Velasco y Adalid Calizaya.
  </div>
</section>

<section class="toc">
  <h2>Contenido</h2>
  <ol>
    <li>Objetivo y alcance</li>
    <li>Arquitectura y estructura</li>
    <li>Juego y experiencia de usuario</li>
    <li>API, Swagger y CRUD</li>
    <li>Base de datos y migración</li>
    <li>Dockerfile e imagen publicada</li>
    <li>Compose de construcción</li>
    <li>Compose de despliegue</li>
    <li>Salud, persistencia y volumen</li>
    <li>Git y GitHub</li>
    <li>Uso de IA y verificación</li>
    <li>Checklist de la consigna</li>
    <li>Conclusiones y reproducción</li>
  </ol>
  <div class="note">
    Las capturas fueron tomadas de la aplicación en ejecución y los bloques de código corresponden a las salidas reales de las pruebas.
    No se incluyen contraseñas ni el contenido de <code>.env</code>.
  </div>
</section>

<section class="section section-first">
  <h2>1. Objetivo y alcance</h2>
  <p class="lead">Implementar una Web API en .NET 10 con PostgreSQL y un juego de navegador, aplicando el patrón de una tabla y un controlador por servidor.</p>
  <p>El juego muestra círculos en un área de juego; cada clic suma un punto. La partida dura 30 segundos y al final se solicita el nombre para guardar el puntaje en el servidor seleccionado. El leaderboard muestra el top 10.</p>
  <table>
    <tr><th>Elemento</th><th>Implementación de Leonardo</th></tr>
    <tr><td>Servidor / país</td><td>Bolivia</td></tr>
    <tr><td>Tabla</td><td><code>leaderboard_bolivia</code></td></tr>
    <tr><td>Controlador</td><td><code>LeaderboardBoliviaController</code></td></tr>
    <tr><td>Ruta base</td><td><code>/api/leaderboard/bolivia</code></td></tr>
    <tr><td>Operaciones</td><td><code>POST</code>, <code>GET</code> lista, <code>GET</code> por ID, <code>PUT</code>, <code>DELETE</code></td></tr>
    <tr><td>Frontend</td><td>HTML, CSS y JavaScript servidos por la misma API en <code>/</code></td></tr>
    <tr><td>Persistencia</td><td>PostgreSQL 16 con migración de Entity Framework Core 10</td></tr>
  </table>
  <div class="note">El selector del juego incluye Bolivia, Perú y Argentina. Si el controlador de un compañero todavía no está integrado, el frontend muestra “Servidor aún no disponible” sin romper la partida.</div>
</section>

<section class="section">
  <h2>2. Arquitectura y estructura</h2>
  <div class="arch">
    <div class="box"><strong>Navegador</strong>HTML + CSS + JS</div>
    <div class="arrow">→</div>
    <div class="box"><strong>API .NET 10</strong>Controllers + Swagger</div>
    <div class="arrow">→</div>
    <div class="box"><strong>EF Core 10</strong>Npgsql</div>
    <div class="arrow">→</div>
    <div class="box"><strong>PostgreSQL 16</strong>leaderboard_bolivia</div>
  </div>
  <p>La API publica el frontend desde <code>wwwroot</code>, expone Swagger y aplica las migraciones al arrancar. Docker Compose conecta la API con PostgreSQL mediante la red <code>proyectotecweb_appnet</code> y conserva los datos en el volumen <code>proyectotecweb_pgdata</code>.</p>
  <h3>Estructura relevante</h3>
  <pre>ProyectoTecWeb/
├─ contructor.yml          # build desde Dockerfile
├─ despliegue.yml          # imagen publicada, sin build
├─ .env.example
├─ .gitignore
├─ README.md
├─ scripts/test-crud.ps1
├─ docs/
│  ├─ evidencias.pdf
│  ├─ evidencias.html
│  ├─ img/                 # capturas
│  └─ outputs/             # salidas de pruebas
└─ ProyectoTecWeb/
   ├─ Dockerfile
   ├─ Program.cs
   ├─ Controllers/LeaderboardBoliviaController.cs
   ├─ Data/AppDbContext.cs
   ├─ Data/Configurations/ScoreBoliviaConfiguration.cs
   ├─ Migrations/20261115163458_InitialCreate.cs
   └─ wwwroot/             # juego</pre>
  <h3>Versiones usadas</h3>
  <table>
    <tr><th>Herramienta</th><th>Versión / resultado</th></tr>
    <tr><td>.NET SDK</td><td><code>%%DOTNET_VERSION%%</code></td></tr>
    <tr><td>Docker</td><td><code>%%DOCKERVERSION%%</code></td></tr>
    <tr><td>Docker Compose</td><td><code>%%COMPOSEVERSION%%</code></td></tr>
    <tr><td>Compilación Release</td><td><span class="ok">0 warnings, 0 errores</span></td></tr>
  </table>
</section>

<section class="section">
  <h2>3. Juego y experiencia de usuario</h2>
  <div class="fig">
    <img src="img/01-inicio-leaderboard.png" alt="Pantalla inicial del juego y leaderboard">
    <figcaption><strong>Figura 1.</strong> Pantalla inicial con el selector de servidor (Bolivia), temporizador de 30 s y leaderboard. Se ven los registros guardados de Leonardo Julio (46 pts) y Leonardo (30 pts).</figcaption>
  </div>
  <div class="fig">
    <img src="img/02-juego.png" alt="Partida en curso">
    <figcaption><strong>Figura 2.</strong> Partida en curso: círculos dentro del área de juego, tiempo restante y puntaje parcial. El leaderboard se actualiza desde la API.</figcaption>
  </div>
  <div class="fig">
    <img src="img/03-fin-guardar.png" alt="Fin de partida y guardado">
    <figcaption><strong>Figura 3.</strong> Fin de partida con 46 puntos y el nombre listo para guardar. El botón envía el puntaje a <code>POST /api/leaderboard/bolivia</code>; después se muestra el mensaje “Puntuación guardada en Bolivia”.</figcaption>
  </div>
  <h3>Reglas implementadas</h3>
  <ul>
    <li>Duración de 30 segundos; cada círculo vale 1 punto.</li>
    <li>El nombre es obligatorio y se recorta antes de guardar (máximo 30 caracteres en la tabla).</li>
    <li>El servidor se elige en el encabezado; cada servidor usa su propia ruta y tabla.</li>
    <li>El top 10 se ordena por puntaje descendente y, en empate, por fecha de creación.</li>
    <li>La interfaz es responsive y no requiere frameworks de frontend.</li>
  </ul>
</section>

<section class="section">
  <h2>4. API, Swagger y CRUD</h2>
  <table>
    <tr><th>Método</th><th>Ruta</th><th>Descripción</th><th>Respuesta esperada</th></tr>
    <tr><td><code>POST</code></td><td><code>/api/leaderboard/bolivia</code></td><td>Crea un puntaje</td><td>201 Created</td></tr>
    <tr><td><code>GET</code></td><td><code>/api/leaderboard/bolivia?limit=10</code></td><td>Lista los mejores puntajes</td><td>200 OK</td></tr>
    <tr><td><code>GET</code></td><td><code>/api/leaderboard/bolivia/{id}</code></td><td>Obtiene un puntaje por ID</td><td>200 / 404</td></tr>
    <tr><td><code>PUT</code></td><td><code>/api/leaderboard/bolivia/{id}</code></td><td>Actualiza nombre y puntaje</td><td>204 No Content</td></tr>
    <tr><td><code>DELETE</code></td><td><code>/api/leaderboard/bolivia/{id}</code></td><td>Elimina un puntaje</td><td>204 No Content</td></tr>
  </table>
  <div class="fig small">
    <img src="img/04-swagger.png" alt="Swagger UI">
    <figcaption><strong>Figura 4.</strong> Swagger UI en <code>/swagger</code>, con los cinco endpoints del controlador de Bolivia.</figcaption>
  </div>
  <h3>Salida real de la prueba CRUD</h3>
  <p>El script <code>scripts/test-crud.ps1</code> prueba salud, creación, listado, consulta por ID, actualización, eliminación y el 404 posterior:</p>
  %%CRUD%%
  <h3>Extracto del controlador</h3>
  %%CONTROLLER%%
  <div class="note">Validaciones: nombre no vacío en <code>POST</code>/<code>PUT</code>, <code>score &gt;= 0</code> en la base de datos y respuestas 404 cuando el ID no existe.</div>
</section>

<section class="section">
  <h2>5. Base de datos y migración</h2>
  <p>La tabla se crea con una migración de Entity Framework Core. El nombre de tabla y columnas se define explícitamente en <code>ScoreBoliviaConfiguration</code>.</p>
  <h3>Migración <code>InitialCreate</code></h3>
  %%MIGRATION%%
  <h3>Esquema real en PostgreSQL</h3>
  %%SCHEMA%%
  <h3>Filas actuales del leaderboard</h3>
  %%DBTOP%%
  <p>El índice descendente por <code>score</code> acelera el top 10. La restricción <code>ck_leaderboard_bolivia_score</code> impide puntajes negativos.</p>
  <h3>Respuesta JSON de la API</h3>
  %%LEADERBOARD%%
</section>

<section class="section">
  <h2>6. Dockerfile e imagen publicada</h2>
  <p>El <code>Dockerfile</code> está junto a <code>ProyectoTecWeb.csproj</code>. Usa la imagen SDK 10.0 para restaurar y compilar en Release, y la imagen aspnet 10.0 para ejecutar el DLL compilado. El puerto y la cadena de conexión se reciben por variables de entorno.</p>
  <h3>Dockerfile</h3>
  %%DOCKERFILE%%
  <h3>Compilación Release</h3>
  %%BUILD%%
  <h3>Imágenes locales publicadas</h3>
  %%IMAGETAGS%%
  <div class="fig small">
    <img src="img/05-dockerhub-tags.png" alt="Docker Hub tags">
    <figcaption><strong>Figura 5.</strong> Repositorio público de Docker Hub con las etiquetas <code>latest</code> y <code>1.0.0</code> (linux/amd64, 105 MB comprimida).</figcaption>
  </div>
  <table>
    <tr><th>Dato</th><th>Valor</th></tr>
    <tr><td>Repositorio</td><td>https://hub.docker.com/r/nardolj/proyectowebtec</td></tr>
    <tr><td>Etiqueta exacta</td><td><code>1.0.0</code> (también <code>latest</code>)</td></tr>
    <tr><td>Digest linux/amd64</td><td><code>%%DIGEST%%</code></td></tr>
    <tr><td>Tamaño comprimido</td><td>%%COMPRESSEDMB%% MB</td></tr>
    <tr><td>Último push</td><td>%%PUSHEDAT%%</td></tr>
  </table>
  <div class="note">No hay credenciales dentro de la imagen: la cadena de conexión se inyecta desde <code>.env</code> en tiempo de ejecución y <code>.env</code> está excluido por <code>.gitignore</code> y <code>.dockerignore</code>.</div>
</section>

<section class="section">
  <h2>7. Compose de construcción</h2>
  <p><code>contructor.yml</code> construye la imagen desde el Dockerfile y levanta la API y PostgreSQL en una red compartida, con un volumen nombrado y healthcheck para la base de datos.</p>
  %%CONSTRUCTOR%%
  <h3>Validación del archivo</h3>
  %%CONSTRUCTORCHECK%%
  <p>El archivo contiene la clave <code>build</code>, por lo que <code>docker compose ... up --build -d</code> compila desde el código fuente. El host de PostgreSQL dentro de Compose es el nombre del servicio: <code>postgres</code>.</p>
</section>

<section class="section">
  <h2>8. Compose de despliegue</h2>
  <p><code>despliegue.yml</code> no compila: usa directamente la imagen publicada <code>nardolj/proyectowebtec:1.0.0</code>. Comparte el mismo nombre de proyecto, red, volumen y nombres de contenedor que el stack de construcción, por lo que solo se levanta uno a la vez.</p>
  %%DEPLOY%%
  <h3>Validación: no existe la clave <code>build</code></h3>
  %%DEPLOYCHECK%%
  <h3>Descarga desde Docker Hub</h3>
  %%PULL%%
  <h3>Contenedores en ejecución</h3>
  %%PS%%
  <div class="note">El despliegue se probó después de bajar el stack de construcción con <code>docker compose ... down</code> (sin <code>-v</code>) y luego ejecutar <code>pull</code> y <code>up -d</code>. La API y PostgreSQL quedaron saludables.</div>
</section>

<section class="section">
  <h2>9. Salud, persistencia y volumen</h2>
  <h3>Health checks</h3>
  %%HEALTH%%
  %%HEALTHDB%%
  <h3>Prueba de persistencia</h3>
  <p>Se creó un registro temporal, se bajó el stack sin borrar el volumen y se volvió a levantar. Luego se consultó el mismo ID y se eliminó el registro de prueba:</p>
  %%PERSISTENCE%%
  <div class="note">El volumen <code>proyectotecweb_pgdata</code> se conservó. Los registros reales del leaderboard (46 y 30 puntos) siguen disponibles después del despliegue con la imagen publicada.</div>
</section>

<section class="section">
  <h2>10. Git y GitHub</h2>
  <div class="fig small">
    <img src="img/06-github.png" alt="Repositorio público en GitHub">
    <figcaption><strong>Figura 6.</strong> Repositorio público <code>NardoLj/ProyectoWebTec</code> con el código, los archivos Compose, el README y las evidencias.</figcaption>
  </div>
  <h3>Historial local</h3>
  %%GITLOG%%
  <h3>Remoto</h3>
  %%GITHUBREMOTE%%
  <h3>Archivos versionados</h3>
  %%GITFILES%%
  <h3>Verificación de exclusiones</h3>
  %%GITIGNORE%%
  <div class="note">El repositorio es público. <code>.env</code>, <code>bin/</code> y <code>obj/</code> no están versionados; <code>.env.example</code> sí se incluye como plantilla segura.</div>
</section>

<section class="section">
  <h2>11. Uso de IA y verificación</h2>
  <p>Se utilizó IA como apoyo para generar la estructura inicial del proyecto, el juego web, el controlador, la migración y los archivos de despliegue. Todo el material generado fue compilado, ejecutado y probado antes de la entrega.</p>
  <h3>Consulta principal</h3>
  <div class="note">“Generar un Compose de despliegue para una API .NET 10 publicada como <code>nardolj/proyectowebtec:1.0.0</code> y PostgreSQL 16, usando una red compartida, volumen nombrado, healthcheck y variables desde <code>.env</code>, sin compilar desde el código fuente.”</div>
  <h3>Verificación realizada</h3>
  <ol>
    <li><code>dotnet build -c Release</code>: 0 warnings y 0 errores.</li>
    <li><code>docker compose -f contructor.yml config</code>: archivo válido y con clave <code>build</code>.</li>
    <li><code>docker compose -f despliegue.yml config</code>: archivo válido y sin clave <code>build</code>.</li>
    <li><code>docker compose -f despliegue.yml pull</code>: imagen publicada descargada.</li>
    <li><code>up -d</code> y <code>ps</code>: API y PostgreSQL en ejecución; PostgreSQL healthy.</li>
    <li><code>GET /health</code>, <code>GET /health/db</code> y el leaderboard: HTTP 200.</li>
    <li>CRUD completo: 201, 200, 204, 204 y 404 posterior al DELETE.</li>
    <li>Persistencia tras <code>down</code> sin <code>-v</code>: registro conservado en el volumen nombrado.</li>
    <li>Push a Docker Hub con etiquetas <code>1.0.0</code> y <code>latest</code>; push a GitHub en <code>main</code>.</li>
  </ol>
  <p>La IA no se usó como sustituto de las pruebas: cada resultado de este documento fue obtenido ejecutando los comandos en el entorno local con Docker Desktop y .NET 10.</p>
</section>

<section class="section">
  <h2>12. Checklist de la consigna</h2>
  <table class="checklist">
    <tr><th>Requisito</th><th>Evidencia</th><th>Estado</th></tr>
    <tr><td>Web API .NET 10</td><td>Compilación Release y Swagger</td><td class="ok">Cumplido</td></tr>
    <tr><td>PostgreSQL + migraciones</td><td>Esquema real y migración</td><td class="ok">Cumplido</td></tr>
    <tr><td>CRUD completo</td><td>Salida 201/200/204/404</td><td class="ok">Cumplido</td></tr>
    <tr><td>Tabla y controlador de Bolivia</td><td><code>leaderboard_bolivia</code> y ruta propia</td><td class="ok">Cumplido</td></tr>
    <tr><td>Juego de círculos</td><td>Figuras 1, 2 y 3</td><td class="ok">Cumplido</td></tr>
    <tr><td>Nombre y guardado de puntaje</td><td>Figura 3 y POST del CRUD</td><td class="ok">Cumplido</td></tr>
    <tr><td>Selector de servidor</td><td>Figura 1 y rutas del frontend</td><td class="ok">Cumplido</td></tr>
    <tr><td>Dockerfile junto al .csproj</td><td><code>ProyectoTecWeb/Dockerfile</code></td><td class="ok">Cumplido</td></tr>
    <tr><td>Imagen en Docker Hub</td><td>Figura 5 y digest</td><td class="ok">Cumplido</td></tr>
    <tr><td><code>contructor.yml</code> con build</td><td>Sección 7</td><td class="ok">Cumplido</td></tr>
    <tr><td><code>despliegue.yml</code> sin build</td><td>Sección 8</td><td class="ok">Cumplido</td></tr>
    <tr><td>Red, volumen y healthcheck</td><td>Archivos Compose y <code>ps</code></td><td class="ok">Cumplido</td></tr>
    <tr><td>GitHub público y <code>.gitignore</code></td><td>Figura 6 y verificaciones</td><td class="ok">Cumplido</td></tr>
    <tr><td>Documento PDF</td><td>Este archivo</td><td class="ok">Cumplido</td></tr>
    <tr><td>ZIP sin <code>bin</code></td><td><code>ProyectoTecWeb-LeonardoJulio.zip</code></td><td class="ok">Cumplido</td></tr>
  </table>
  <p class="small">Los servidores de Perú y Argentina quedan asignados a los otros integrantes; el frontend ya contempla sus rutas y muestra un aviso si aún no están disponibles.</p>
</section>

<section class="section">
  <h2>13. Conclusiones y reproducción</h2>
  <p>La parte de Leonardo Julio quedó implementada y verificada de extremo a extremo: el juego funciona en el navegador, la API expone el CRUD de Bolivia, PostgreSQL conserva los datos en un volumen nombrado, la imagen está publicada en Docker Hub y el repositorio está en GitHub.</p>
  <p>La separación por servidor se mantiene mediante la ruta <code>/api/leaderboard/bolivia</code> y la tabla <code>leaderboard_bolivia</code>, dejando el mismo contrato preparado para Perú y Argentina.</p>
  <h3>Comandos para reproducir</h3>
  <pre>Copy-Item .env.example .env
# Editar .env y definir una contraseña segura.

docker compose --env-file .env -f contructor.yml up --build -d
docker compose --env-file .env -f contructor.yml ps

# Alternativa con la imagen publicada:
docker compose --env-file .env -f contructor.yml down
docker compose --env-file .env -f despliegue.yml pull
docker compose --env-file .env -f despliegue.yml up -d

# Pruebas:
powershell -ExecutionPolicy Bypass -File .\scripts\test-crud.ps1
curl.exe http://localhost:8080/health
curl.exe http://localhost:8080/api/leaderboard/bolivia?limit=10</pre>
  <h3>Enlaces</h3>
  <ul>
    <li>Repositorio: https://github.com/NardoLj/ProyectoWebTec</li>
    <li>Docker Hub: https://hub.docker.com/r/nardolj/proyectowebtec</li>
    <li>Juego local: http://localhost:8080/</li>
    <li>Swagger local: http://localhost:8080/swagger</li>
  </ul>
  <div class="stamp">Documento generado a partir de las capturas y salidas de las pruebas del 15/11/2026.</div>
</section>

</body>
</html>
'@

$tokens = [ordered]@{
    '%%DIGEST%%' = [System.Net.WebUtility]::HtmlEncode($digest)
    '%%COMPRESSEDMB%%' = [System.Net.WebUtility]::HtmlEncode([string]$compressedMb)
    '%%PUSHEDAT%%' = [System.Net.WebUtility]::HtmlEncode([string]$pushedAt)
    '%%DOTNET_VERSION%%' = [System.Net.WebUtility]::HtmlEncode($dotnetVersion)
    '%%DOCKERVERSION%%' = [System.Net.WebUtility]::HtmlEncode($dockerVersion)
    '%%COMPOSEVERSION%%' = [System.Net.WebUtility]::HtmlEncode($composeVersion)
    '%%CRUD%%' = OutputCode '08-crud.txt'
    '%%CONTROLLER%%' = OutputCode '20-controller-excerpt.txt'
    '%%MIGRATION%%' = FileCode 'ProyectoTecWeb/Migrations/20261115163458_InitialCreate.cs'
    '%%SCHEMA%%' = OutputCode '16-db-schema.txt'
    '%%DBTOP%%' = OutputCode '17-db-top.txt'
    '%%LEADERBOARD%%' = OutputCode '10-leaderboard.json'
    '%%DOCKERFILE%%' = FileCode 'ProyectoTecWeb/Dockerfile'
    '%%BUILD%%' = OutputCode '02-build.txt'
    '%%IMAGETAGS%%' = OutputCode '15-image-tags.txt'
    '%%CONSTRUCTOR%%' = FileCode 'contructor.yml'
    '%%CONSTRUCTORCHECK%%' = OutputCode '21-constructor-check.txt'
    '%%DEPLOY%%' = FileCode 'despliegue.yml'
    '%%DEPLOYCHECK%%' = OutputCode '18-deployment-check.txt'
    '%%PULL%%' = OutputCode '12-compose-pull.txt'
    '%%PS%%' = OutputCode '11-docker-ps.txt'
    '%%HEALTH%%' = OutputCode '09-health.txt'
    '%%HEALTHDB%%' = OutputCode '09-health-db.txt'
    '%%PERSISTENCE%%' = OutputCode '19-persistence.txt'
    '%%GITLOG%%' = OutputCode '03-git-log.txt'
    '%%GITHUBREMOTE%%' = OutputCode '23-github-remote.txt'
    '%%GITFILES%%' = OutputCode '05-git-files.txt'
    '%%GITIGNORE%%' = OutputCode '22-gitignore-check.txt'
}

foreach ($key in $tokens.Keys) {
    $html = $html.Replace($key, [string]$tokens[$key])
}

[System.IO.File]::WriteAllText($htmlPath, $html, [System.Text.UTF8Encoding]::new($false))
Write-Host "HTML generado: $htmlPath"
