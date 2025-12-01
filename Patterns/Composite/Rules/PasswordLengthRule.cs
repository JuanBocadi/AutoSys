namespace AutoSys.Patterns.Composite.Rules
{
    /// <summary>
    /// Regla de validación para longitud mínima de contraseña (Leaf del patrón Composite)
    /// </summary>
    public class PasswordLengthRule : IValidationComponent
    {
        private readonly string _fieldName;
        private readonly int _minLength;

        public string Name => $"Longitud mínima de contraseña ({_minLength} caracteres)";

        public PasswordLengthRule(int minLength = 6, string fieldName = "Password")
        {
            _minLength = minLength;
            _fieldName = fieldName;
        }

        public ValidationResult Validate(Dictionary<string, object?> context)
        {
            if (!context.TryGetValue(_fieldName, out var value) || value == null)
            {
                return ValidationResult.Success();
            }

            var password = value.ToString();
            if (string.IsNullOrEmpty(password))
            {
                return ValidationResult.Success();
            }

            if (password.Length < _minLength)
            {
                return ValidationResult.Failure($"La contraseña debe tener al menos {_minLength} caracteres.");
            }

            return ValidationResult.Success();
        }
    }
}
