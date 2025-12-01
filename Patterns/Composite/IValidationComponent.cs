namespace AutoSys.Patterns.Composite
{
    /// <summary>
    /// Patrón Composite: Componente base para validaciones
    /// Permite crear validaciones simples y compuestas de forma jerárquica
    /// </summary>
    public interface IValidationComponent
    {
        bool Validate(ValidationContext context);
        string GetErrorMessage();
    }

    public class ValidationContext
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Rol { get; set; }
    }
}
