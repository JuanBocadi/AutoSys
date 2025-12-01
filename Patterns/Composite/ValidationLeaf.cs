namespace AutoSys.Patterns.Composite
{
    /// <summary>
    /// Patrón Composite: Hoja - Validación individual (elemento simple)
    /// </summary>
    public abstract class ValidationLeaf : IValidationComponent
    {
        protected string _errorMessage = string.Empty;

        public abstract bool Validate(ValidationContext context);

        public string GetErrorMessage() => _errorMessage;
    }

    // Validaciones concretas
    public class UsernameRequiredValidation : ValidationLeaf
    {
        public override bool Validate(ValidationContext context)
        {
            if (string.IsNullOrWhiteSpace(context.Username))
            {
                _errorMessage = "El nombre de usuario es obligatorio.";
                return false;
            }
            return true;
        }
    }

    public class EmailFormatValidation : ValidationLeaf
    {
        public override bool Validate(ValidationContext context)
        {
            if (!string.IsNullOrEmpty(context.Email) && !context.Email.Contains("@"))
            {
                _errorMessage = "El formato del email no es válido.";
                return false;
            }
            return true;
        }
    }

    public class PasswordLengthValidation : ValidationLeaf
    {
        private readonly int _minLength;

        public PasswordLengthValidation(int minLength = 6)
        {
            _minLength = minLength;
        }

        public override bool Validate(ValidationContext context)
        {
            if (string.IsNullOrEmpty(context.Password) || context.Password.Length < _minLength)
            {
                _errorMessage = $"La contraseña debe tener al menos {_minLength} caracteres.";
                return false;
            }
            return true;
        }
    }

    public class PasswordComplexityValidation : ValidationLeaf
    {
        public override bool Validate(ValidationContext context)
        {
            if (string.IsNullOrEmpty(context.Password))
            {
                _errorMessage = "La contraseña es obligatoria.";
                return false;
            }

            bool hasUpper = context.Password.Any(char.IsUpper);
            bool hasLower = context.Password.Any(char.IsLower);
            bool hasDigit = context.Password.Any(char.IsDigit);

            if (!hasUpper || !hasLower || !hasDigit)
            {
                _errorMessage = "La contraseña debe contener mayúsculas, minúsculas y números.";
                return false;
            }
            return true;
        }
    }

    public class RoleRequiredValidation : ValidationLeaf
    {
        public override bool Validate(ValidationContext context)
        {
            if (string.IsNullOrWhiteSpace(context.Rol))
            {
                _errorMessage = "Debe seleccionar un rol.";
                return false;
            }
            return true;
        }
    }
}
