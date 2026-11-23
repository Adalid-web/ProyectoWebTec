param(
    [string]$BaseUrl = "http://localhost:8080",
    [string]$Server = "bolivia",
    [string]$Cookie = ""
)

# Prueba el leaderboard de un servidor.
#
# Sin -Cookie: verifica que la API responda 401 sin sesion (proteccion de endpoints).
# Con -Cookie: prueba POST, GET lista, GET por ID, PUT, DELETE logico y 404 posterior.
#
# Como obtener la cookie:
#   1. Iniciar sesion en http://localhost:8080/ con Google o Discord.
#   2. Abrir DevTools (F12) -> Application/Storage -> Cookies -> http://localhost:8080.
#   3. Copiar el valor de la cookie "ProyectoTecWeb.Auth".
#   4. Ejecutar:
#      .\scripts\test-crud.ps1 -Cookie "ProyectoTecWeb.Auth=VALOR"
#
# No pegar la cookie en capturas ni en el repositorio.

$ErrorActionPreference = "Stop"
$endpoint = "$BaseUrl/api/leaderboard/$Server"
$failures = 0
$headers = @{}

if (-not [string]::IsNullOrWhiteSpace($Cookie)) {
    $headers.Cookie = $Cookie
}

function Invoke-Api {
    param(
        [string]$Method,
        [string]$Url,
        [string]$Body = $null
    )

    try {
        $parameters = @{
            Uri = $Url
            Method = $Method
            UseBasicParsing = $true
            Headers = $headers
        }

        if ($Body) {
            $parameters.Body = $Body
            $parameters.ContentType = "application/json"
        }

        $response = Invoke-WebRequest @parameters
        return [pscustomobject]@{
            Status = [int]$response.StatusCode
            Content = $response.Content
        }
    }
    catch {
        $response = $_.Exception.Response
        if ($null -eq $response) {
            throw
        }

        $reader = New-Object System.IO.StreamReader($response.GetResponseStream())
        $content = $reader.ReadToEnd()
        $reader.Close()

        return [pscustomobject]@{
            Status = [int]$response.StatusCode
            Content = $content
        }
    }
}

function Check {
    param(
        [string]$Name,
        [object]$Expected,
        [object]$Actual
    )

    if ($Expected -eq $Actual) {
        Write-Host "[OK]   $Name -> $Actual" -ForegroundColor Green
        return $true
    }

    Write-Host "[FAIL] $Name -> esperado $Expected, recibido $Actual" -ForegroundColor Red
    return $false
}

Write-Host ""
Write-Host "Probando leaderboard de $Server en $BaseUrl" -ForegroundColor Cyan
Write-Host "------------------------------------------------"

$health = Invoke-Api -Method Get -Url "$BaseUrl/health"
if (-not (Check -Name "GET /health" -Expected 200 -Actual $health.Status)) { $failures++ }

$dbHealth = Invoke-Api -Method Get -Url "$BaseUrl/health/db"
if (-not (Check -Name "GET /health/db" -Expected 200 -Actual $dbHealth.Status)) { $failures++ }

if ([string]::IsNullOrWhiteSpace($Cookie)) {
    Write-Host ""
    Write-Host "No se recibio -Cookie: se verificara el rechazo sin sesion." -ForegroundColor Yellow
    $unauthorized = Invoke-Api -Method Get -Url "$endpoint`?limit=1"
    if (-not (Check -Name "GET sin sesion" -Expected 401 -Actual $unauthorized.Status)) { $failures++ }

    Write-Host ""
    Write-Host "Para probar el CRUD, pasa la cookie de sesion con -Cookie." -ForegroundColor Yellow
    Write-Host "Instrucciones en el encabezado de este script." -ForegroundColor DarkGray

    Write-Host "------------------------------------------------"
    if ($failures -eq 0) {
        Write-Host "Proteccion sin sesion de ${Server}: OK" -ForegroundColor Green
        exit 0
    }

    Write-Host "Proteccion sin sesion de ${Server}: $failures fallo(s)" -ForegroundColor Red
    exit 1
}

$authorized = Invoke-Api -Method Get -Url "$endpoint`?limit=1"
if (-not (Check -Name "GET con sesion" -Expected 200 -Actual $authorized.Status)) {
    Write-Host "La cookie no es valida o expiro. Inicia sesion de nuevo." -ForegroundColor Red
    exit 1
}

$name = "Test-" + (Get-Date -Format "HHmmss")
$createBody = @{ playerName = $name; score = 17 } | ConvertTo-Json -Compress
$created = Invoke-Api -Method Post -Url $endpoint -Body $createBody
if (-not (Check -Name "POST crear" -Expected 201 -Actual $created.Status)) {
    $failures++
    Write-Host "No se puede continuar sin crear un registro." -ForegroundColor Red
    exit 1
}

$createdData = $created.Content | ConvertFrom-Json
$id = [int]$createdData.id
if (-not (Check -Name "POST devuelve id" -Expected $true -Actual ($id -gt 0))) { $failures++ }
Write-Host "       id creado: $id" -ForegroundColor DarkGray

$list = Invoke-Api -Method Get -Url "$endpoint`?limit=100"
if (-not (Check -Name "GET lista" -Expected 200 -Actual $list.Status)) { $failures++ }

$listData = $list.Content | ConvertFrom-Json
$found = @($listData | Where-Object { [int]$_.id -eq $id }).Count -gt 0
if (-not (Check -Name "GET lista contiene el id" -Expected $true -Actual $found)) { $failures++ }

$byId = Invoke-Api -Method Get -Url "$endpoint/$id"
if (-not (Check -Name "GET por id" -Expected 200 -Actual $byId.Status)) { $failures++ }

$updateBody = @{ playerName = "$name-updated"; score = 23 } | ConvertTo-Json -Compress
$updated = Invoke-Api -Method Put -Url "$endpoint/$id" -Body $updateBody
if (-not (Check -Name "PUT actualizar" -Expected 204 -Actual $updated.Status)) { $failures++ }

$byIdUpdated = Invoke-Api -Method Get -Url "$endpoint/$id"
$updatedData = $byIdUpdated.Content | ConvertFrom-Json
if (-not (Check -Name "GET refleja el puntaje actualizado" -Expected 23 -Actual ([int]$updatedData.score))) { $failures++ }

$deleted = Invoke-Api -Method Delete -Url "$endpoint/$id"
if (-not (Check -Name "DELETE soft delete" -Expected 204 -Actual $deleted.Status)) { $failures++ }

$missing = Invoke-Api -Method Get -Url "$endpoint/$id"
if (-not (Check -Name "GET posterior a DELETE" -Expected 404 -Actual $missing.Status)) { $failures++ }

$updateDeleted = Invoke-Api -Method Put -Url "$endpoint/$id" -Body $updateBody
if (-not (Check -Name "PUT sobre eliminado" -Expected 404 -Actual $updateDeleted.Status)) { $failures++ }

$deleteAgain = Invoke-Api -Method Delete -Url "$endpoint/$id"
if (-not (Check -Name "DELETE sobre eliminado" -Expected 404 -Actual $deleteAgain.Status)) { $failures++ }

Write-Host "------------------------------------------------"
Write-Host "Recuerda comprobar en PostgreSQL que la fila sigue existiendo con is_deleted = true." -ForegroundColor DarkGray
if ($failures -eq 0) {
    Write-Host "CRUD de ${Server}: OK" -ForegroundColor Green
    exit 0
}

Write-Host "CRUD de ${Server}: $failures fallo(s)" -ForegroundColor Red
exit 1
