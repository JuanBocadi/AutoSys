param(
    [int]$Port = 5206,
    [string]$Region = "us"
)

# Ruta al proyecto
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$projectRoot = Resolve-Path (Join-Path $scriptDir "..")
$projectFile = Join-Path $projectRoot "AutoSys.csproj"

Write-Host "Iniciando la aplicación (dotnet run) en background..."
Start-Process -FilePath "dotnet" -ArgumentList "run","--project","$projectFile" -WorkingDirectory $projectRoot

Start-Sleep -Seconds 3

Write-Host "Iniciando ngrok en puerto $Port (región: $Region)..."
Write-Host "Ejecutando: ngrok http $Port --region $Region"

# Iniciar ngrok
Start-Process -FilePath "ngrok" -ArgumentList "http",$Port,"--region",$Region

Write-Host ""
Write-Host "ngrok iniciado. Para ver la URL pública:"
Write-Host "  - Abre http://localhost:4040 en tu navegador (ngrok Web Interface)"
Write-Host "  - O ejecuta: curl http://localhost:4040/api/tunnels"
Write-Host ""
Write-Host "Nota: ambos procesos (dotnet y ngrok) seguirán corriendo en ventanas separadas."
