namespace AutoSys.Patterns.Composite.Rules
{
    /// <summary>
    /// Regla de validación para confirmar contraseña (Leaf del patrón Composite)
    /// </summary>
    public class PasswordMatchRule : IValidationComponent
    {
        private readonly string _passwordField;
        private readonly string _confirmPasswordField;

        public string Name => "Coincidencia de contraseñas";

        public PasswordMatchRule(string passwordField = "Password", string confirmPasswordField = "ConfirmPassword")
        {
            _passwordField = passwordField;
            _confirmPasswordField = confirmPasswordField;
        }

        public ValidationResult Validate(Dictionary<string, object?> context)
        {
            context.TryGetValue(_passwordField, out var password);
            context.TryGetValue(_confirmPasswordField, out var confirmPassword);

            var pass = password?.ToString() ?? string.Empty;
            var confirm = confirmPassword?.ToString() ?? string.Empty;

            if (!string.Equals(pass, confirm, StringComparison.Ordinal))
            {
                return ValidationResult.Failure("Las contraseñas no coinciden.");
            }

            return ValidationResult.Success();
        }
    }
}
