# Patrones de Diseño Implementados en AutoSys

## Resumen de Patrones Implementados

Este documento detalla los **3 patrones de diseño** implementados en el sistema AutoSys:

1. **Composite** - Validaciones jerárquicas en Login/Register
2. **Singleton** - Gestor de configuración de la aplicación
3. **Factory Method** - Generación de diferentes tipos de reportes

---

## 1. Patrón COMPOSITE

**Ubicación:** `Patterns/Composite/`

**Propósito:** Permite componer objetos en estructuras de árbol para representar jerarquías de validaciones. Permite tratar validaciones individuales y composiciones de validaciones de manera uniforme.

### Implementación

#### Componente Base: `IValidationComponent`
```csharp
public interface IValidationComponent
{
    bool Validate(ValidationContext context);
    string GetErrorMessage();
}
```

#### Hojas (Leaf): Validaciones concretas
- `UsernameRequiredValidation`
- `EmailFormatValidation`
- `PasswordLengthValidation`
- `PasswordComplexityValidation`
- `RoleRequiredValidation`

#### Composite: `ValidationComposite`
Agrupa múltiples validaciones y las ejecuta secuencialmente.

### Uso en el Sistema

**AccountController.cs - Login:**
```csharp
var validationContext = new ValidationContext
{
    Username = email,
    Password = password
};

var validator = ValidationFactory.CreateLoginValidation();
if (!validator.Validate(validationContext))
{
    ViewBag.Error = validator.GetErrorMessage();
    return View();
}
```

**AccountController.cs - Register:**
```csharp
var validationContext = new ValidationContext
{
    Username = model.Username,
    Email = model.Email,
    Password = model.Password,
    Rol = model.Rol
};

var validator = ValidationFactory.CreateRegisterValidation();
if (!validator.Validate(validationContext))
{
    ModelState.AddModelError(string.Empty, validator.GetErrorMessage());
    return View(model);
}
```

### Ventajas
- Facilita agregar nuevas validaciones sin modificar código existente
- Permite crear validaciones complejas combinando validaciones simples
- Código más limpio y mantenible
- Cumple con el principio Open/Closed

---

## 2. Patrón SINGLETON

**Ubicación:** `Patterns/Singleton/AppConfigurationManager.cs`

**Propósito:** Garantiza que una clase tenga una única instancia y proporciona un punto de acceso global a ella. Se usa para gestionar la configuración de la aplicación.

### Implementación

```csharp
public sealed class AppConfigurationManager
{
    private static readonly Lazy<AppConfigurationManager> _instance = 
        new Lazy<AppConfigurationManager>(() => new AppConfigurationManager());

    private readonly Dictionary<string, string> _settings;

    private AppConfigurationManager()
    {
        _settings = new Dictionary<string, string>
        {
            { "SessionTimeout", "15" },
            { "MaxFileSize", "5242880" },
            { "AllowedFileExtensions", ".jpg,.jpeg,.png,.pdf" },
            { "DefaultRole", "Recepcionista" },
            { "MinPasswordLength", "6" },
            { "EnableEmailNotifications", "false" },
            { "ReportsPageSize", "20" },
            { "StockWarningLevel", "10" }
        };
    }

    public static AppConfigurationManager Instance => _instance.Value;
}
```

### Características
- **Thread-Safe:** Usa `Lazy<T>` para inicialización segura en entornos multi-hilo
- **Sellada:** La clase es `sealed` para prevenir herencia
- Constructor privado para prevenir instanciación externa

### Uso en el Sistema

**AccountController.cs - Login:**
```csharp
var config = AppConfigurationManager.Instance;
int minPasswordLength = config.GetSettingAsInt("MinPasswordLength", 6);
```

**ReportesController.cs - StockBajo:**
```csharp
var config = AppConfigurationManager.Instance;
int warningLevel = config.GetSettingAsInt("StockWarningLevel", 10);
```

### Ventajas
- Acceso global a la configuración desde cualquier parte de la aplicación
- Ahorro de memoria (una sola instancia)
- Control centralizado de la configuración
- Thread-safe

---

## 3. Patrón FACTORY METHOD

**Ubicación:** `Patterns/Factory/`

**Propósito:** Define una interfaz para crear objetos, pero deja que las subclases decidan qué clase instanciar. Permite delegar la lógica de instanciación a las subclases.

### Implementación

#### Producto Base: `IReport`
```csharp
public interface IReport
{
    string GetTitle();
    string GetDescription();
    Dictionary<string, object> GenerateData();
    string GetReportType();
}
```

#### Productos Concretos
- `ClientesActivosReport`
- `FacturacionPeriodoReport`
- `IngresosPeriodoReport`
- `StockBajoReport`
- `TiemposReparacionReport`

#### Creador Abstracto: `ReportFactory`
```csharp
public abstract class ReportFactory
{
    protected readonly AutoSysDbContext _context;

    public abstract IReport CreateReport();

    public Dictionary<string, object> GenerateReport()
    {
        var report = CreateReport();
        return report.GenerateData();
    }
}
```

#### Creadores Concretos
- `ClientesActivosReportFactory`
- `FacturacionPeriodoReportFactory`
- `IngresosPeriodoReportFactory`
- `StockBajoReportFactory`
- `TiemposReparacionReportFactory`

### Uso en el Sistema

**ReportesController.cs - IngresosPorPeriodo:**
```csharp
var reportFactory = new IngresosPeriodoReportFactory(_context, desde.Value, hasta.Value);
var report = reportFactory.CreateReport();
var data = report.GenerateData();

ViewBag.Desde = data["Desde"];
ViewBag.Hasta = data["Hasta"];
ViewBag.TotalIngresos = data["TotalIngresos"];
```

**ReportesController.cs - FacturacionPorPeriodo:**
```csharp
var reportFactory = new FacturacionPeriodoReportFactory(_context, desde.Value, hasta.Value);
var report = reportFactory.CreateReport();
var data = report.GenerateData();
```

**ReportesController.cs - ClientesActivos:**
```csharp
var reportFactory = new ClientesActivosReportFactory(_context);
var report = reportFactory.CreateReport();
var data = report.GenerateData();
```

**ReportesController.cs - StockBajo:**
```csharp
var reportFactory = new StockBajoReportFactory(_context);
var report = reportFactory.CreateReport();
var data = report.GenerateData();
```

**ReportesController.cs - TiemposReparacion:**
```csharp
var reportFactory = new TiemposReparacionReportFactory(_context);
var report = reportFactory.CreateReport();
var data = report.GenerateData();
```

### Ventajas
- Facilita agregar nuevos tipos de reportes sin modificar código existente
- Desacopla la lógica de negocio de la generación de reportes
- Permite polimorfismo en la creación de reportes
- Código más organizado y testeable
- Cumple con el principio Open/Closed

---

## Patrones Adicionales Recomendados (Ya implementados en otras partes)

### Strategy Pattern
Ya implementado implícitamente en los servicios:
- `INotificationService` / `NotificationService`
- `IFileStorage` / `LocalFileStorage`

### Observer Pattern
Podría implementarse en:
- Notificaciones cuando cambia el estado de un ingreso
- Alertas de stock bajo

---

## Diagramas de Clases

### Composite Pattern
```
IValidationComponent
    ├── ValidationLeaf (abstract)
    │   ├── UsernameRequiredValidation
    │   ├── EmailFormatValidation
    │   ├── PasswordLengthValidation
    │   ├── PasswordComplexityValidation
    │   └── RoleRequiredValidation
    └── ValidationComposite
```

### Factory Method Pattern
```
ReportFactory (abstract)
    ├── ClientesActivosReportFactory
    ├── FacturacionPeriodoReportFactory
    ├── IngresosPeriodoReportFactory
    ├── StockBajoReportFactory
    └── TiemposReparacionReportFactory
        ↓ creates
    IReport
        ├── ClientesActivosReport
        ├── FacturacionPeriodoReport
        ├── IngresosPeriodoReport
        ├── StockBajoReport
        └── TiemposReparacionReport
```

### Singleton Pattern
```
AppConfigurationManager (sealed)
    - Instance: AppConfigurationManager (static, readonly, Lazy)
    - _settings: Dictionary<string, string>
    + GetSetting(key): string
    + GetSettingAsInt(key, defaultValue): int
    + GetSettingAsBool(key, defaultValue): bool
    + UpdateSetting(key, value): void
```

---

## Conclusión

Los tres patrones implementados mejoran significativamente la arquitectura del sistema:

1. **Composite:** Hace las validaciones más flexibles y mantenibles
2. **Singleton:** Centraliza la configuración de la aplicación
3. **Factory Method:** Facilita la extensión del sistema de reportes

Todos los patrones cumplen con los principios SOLID y mejoran la calidad del código.
