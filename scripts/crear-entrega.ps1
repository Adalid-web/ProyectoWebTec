param(
    [string]$Output = ""
)

# Genera un ZIP limpio del proyecto, sin secretos ni artefactos de compilación.
# Uso:
#   powershell -ExecutionPolicy Bypass -File .\scripts\crear-entrega.ps1
#   powershell -ExecutionPolicy Bypass -File .\scripts\crear-entrega.ps1 -Output .\ProyectoTecWeb-entrega-final.zip

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($Output)) {
    $Output = Join-Path $root "ProyectoTecWeb-entrega.zip"
} elseif (-not [System.IO.Path]::IsPathRooted($Output)) {
    $Output = Join-Path $root $Output
}

$temp = Join-Path $env:TEMP ("ProyectoTecWeb-entrega-" + [guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $temp | Out-Null

try {
    # Rutas absolutas para que /XD excluya correctamente las carpetas anidadas.
    $excludedDirs = @(
        (Join-Path $root ".git"),
        (Join-Path $root ".vs"),
        (Join-Path $root ".idea"),
        (Join-Path $root "bin"),
        (Join-Path $root "obj"),
        (Join-Path $root "docs\img"),
        (Join-Path $root "docs\outputs")
    )

    $robocopyArgs = @(
        $root,
        $temp,
        "/E",
        "/XD"
    ) + $excludedDirs + @(
        "/XF", ".env", ".env.local", "*.zip", "*.log", "evidencias.pdf", "evidencias.html",
        "/NFL", "/NDL", "/NJH", "/NJS", "/NP"
    )

    & robocopy @robocopyArgs | Out-Null
    if ($LASTEXITCODE -ge 8) {
        throw "Robocopy fallo con codigo $LASTEXITCODE."
    }

    # Refuerzo: eliminar cualquier bin/obj que robocopy no haya excluido.
    Get-ChildItem -Path $temp -Recurse -Directory -Force -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -in @("bin", "obj", ".git", ".vs", ".idea") } |
        Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

    $readme = @"
ProyectoTecWeb - entrega

1. Copiar .env.example a .env.
2. Completar POSTGRES_PASSWORD y las credenciales OAuth que se van a probar.
   No subir .env al repositorio.
3. Ejecutar:
   docker compose --env-file .env -f contructor.yml up --build -d
4. Abrir http://localhost:8080/ y http://localhost:8080/swagger

Documentacion:
- README.md
- CONTRIBUTING.md
- docs/autenticacion.md
- docs/plantilla-evidencias.md

Este ZIP no contiene .env, bin, obj ni el historial .git.
"@

    [System.IO.File]::WriteAllText((Join-Path $temp "LEEME.txt"), $readme, [System.Text.UTF8Encoding]::new($false))

    if (Test-Path $Output) {
        Remove-Item $Output -Force
    }

    Compress-Archive -Path (Join-Path $temp "*") -DestinationPath $Output -CompressionLevel Optimal
    $sizeMb = [math]::Round((Get-Item $Output).Length / 1MB, 2)
    Write-Host "ZIP generado: $Output ($sizeMb MB)" -ForegroundColor Green
}
finally {
    if (Test-Path $temp) {
        Remove-Item $temp -Recurse -Force
    }
}
