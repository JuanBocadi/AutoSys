namespace AutoSys.Patterns.Composite.Rules
{
    /// <summary>
    /// Regla de validación para campos requeridos (Leaf del patrón Composite)
    /// </summary>
    public class RequiredFieldRule : IValidationComponent
    {
        private readonly string _fieldName;
        private readonly string _displayName;

        public string Name => $"Campo requerido: {_displayName}";

        public RequiredFieldRule(string fieldName, string displayName)
        {
            _fieldName = fieldName;
            _displayName = displayName;
        }

        public ValidationResult Validate(Dictionary<string, object?> context)
        {
            if (!context.TryGetValue(_fieldName, out var value) || 
                value == null || 
                string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Failure($"El campo {_displayName} es obligatorio.");
            }

            return ValidationResult.Success();
        }
    }
}
