using System.Text.RegularExpressions;

namespace AutoSys.Patterns.Composite.Rules
{
    /// <summary>
    /// Regla de validación para formato de email (Leaf del patrón Composite)
    /// </summary>
    public class EmailFormatRule : IValidationComponent
    {
        private readonly string _fieldName;
        private static readonly Regex EmailRegex = new(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string Name => "Validación de formato de email";

        public EmailFormatRule(string fieldName = "Email")
        {
            _fieldName = fieldName;
        }

        public ValidationResult Validate(Dictionary<string, object?> context)
        {
            if (!context.TryGetValue(_fieldName, out var value) || value == null)
            {
                return ValidationResult.Success(); // Si no hay valor, otra regla lo validará
            }

            var email = value.ToString();
            if (string.IsNullOrEmpty(email))
            {
                return ValidationResult.Success();
            }

            if (!EmailRegex.IsMatch(email))
            {
                return ValidationResult.Failure("El formato del correo electrónico no es válido.");
            }

            return ValidationResult.Success();
        }
    }
}
