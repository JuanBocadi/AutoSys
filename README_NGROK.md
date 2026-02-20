# Configuración de ngrok para AutoSys

## Requisitos
- Instalar ngrok: https://ngrok.com/download
- (Opcional) Crear cuenta gratuita en ngrok para obtener un authtoken

## Instalación rápida (Windows)

### Opción 1: Chocolatey
```powershell
choco install ngrok
```

### Opción 2: Manual
1. Descarga ngrok desde https://ngrok.com/download
2. Extrae el ejecutable a una carpeta en tu PATH (ej: `C:\Windows\System32` o `C:\Program Files\ngrok`)

### Configurar authtoken (opcional pero recomendado)
```bash
ngrok config add-authtoken TU_TOKEN_AQUI
```
Esto permite sesiones más largas y otras funcionalidades.

## Uso

### Desde VS Code
- Ctrl+Shift+B → selecciona `Run AutoSys + ngrok`
- Esto iniciará la app y ngrok automáticamente

### Desde terminal
```powershell
# Ejecutar el script
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/run-with-ngrok.ps1 -Port 5206 -Region us
```

### Manual
```bash
# Terminal 1: iniciar la app
dotnet run

# Terminal 2: iniciar ngrok
ngrok http 5206
```

## Ver la URL pública
- Abre http://localhost:4040 en tu navegador (ngrok Web Interface)
- Copia la URL pública (ej: https://abc123.ngrok-free.app)

## Regiones disponibles
- `us` - United States (predeterminado)
- `eu` - Europe
- `ap` - Asia/Pacific
- `au` - Australia
- `sa` - South America
- `jp` - Japan
- `in` - India

Ejemplo con región europea:
```powershell
powershell -File scripts/run-with-ngrok.ps1 -Port 5206 -Region eu
```

## Notas
- ngrok gratuito muestra una página de advertencia antes de acceder a tu app
- Para usar dominios custom necesitas cuenta de pago
- La URL pública cambia cada vez que reinicias ngrok (a menos que tengas cuenta de pago con dominios reservados)
