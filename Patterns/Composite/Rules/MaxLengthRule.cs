namespace AutoSys.Patterns.Composite.Rules
{
    /// <summary>
    /// Regla de validación para longitud máxima de un campo (Leaf del patrón Composite)
    /// </summary>
    public class MaxLengthRule : IValidationComponent
    {
        private readonly string _fieldName;
        private readonly string _displayName;
        private readonly int _maxLength;

        public string Name => $"Longitud máxima: {_displayName} ({_maxLength} caracteres)";

        public MaxLengthRule(string fieldName, string displayName, int maxLength)
        {
            _fieldName = fieldName;
            _displayName = displayName;
            _maxLength = maxLength;
        }

        public ValidationResult Validate(Dictionary<string, object?> context)
        {
            if (!context.TryGetValue(_fieldName, out var value) || value == null)
            {
                return ValidationResult.Success();
            }

            var text = value.ToString();
            if (string.IsNullOrEmpty(text))
            {
                return ValidationResult.Success();
            }

            if (text.Length > _maxLength)
            {
                return ValidationResult.Failure($"El campo {_displayName} no puede exceder los {_maxLength} caracteres.");
            }

            return ValidationResult.Success();
        }
    }
}
