using AutoSys.Patterns.Composite.Rules;

namespace AutoSys.Patterns.Composite
{
    /// <summary>
    /// Fábrica que crea los validadores composite para Login y Register.
    /// Utiliza el patrón Composite para agrupar múltiples reglas de validación.
    /// </summary>
    public static class ValidationFactory
    {
        /// <summary>
        /// Crea el validador composite para el proceso de Login.
        /// Agrupa las validaciones de email/usuario y contraseña.
        /// </summary>
        public static ValidationComposite CreateLoginValidator()
        {
            var loginValidator = new ValidationComposite("Validación de Login");

            // Grupo de validaciones para credenciales
            var credentialsGroup = new ValidationComposite("Validación de Credenciales");
            credentialsGroup.Add(new RequiredFieldRule("Email", "Correo electrónico o usuario"));
            credentialsGroup.Add(new RequiredFieldRule("Password", "Contraseña"));

            loginValidator.Add(credentialsGroup);

            return loginValidator;
        }

        /// <summary>
        /// Crea el validador composite para el proceso de Registro.
        /// Agrupa las validaciones de nombre de usuario, email, contraseña y rol.
        /// </summary>
        public static ValidationComposite CreateRegisterValidator()
        {
            var registerValidator = new ValidationComposite("Validación de Registro");

            // Grupo de validaciones para información de usuario
            var userInfoGroup = new ValidationComposite("Información de Usuario");
            userInfoGroup.Add(new RequiredFieldRule("Username", "Nombre de usuario"));
            userInfoGroup.Add(new MaxLengthRule("Username", "Nombre de usuario", 50));

            // Grupo de validaciones para email
            var emailGroup = new ValidationComposite("Validación de Email");
            emailGroup.Add(new RequiredFieldRule("Email", "Correo electrónico"));
            emailGroup.Add(new EmailFormatRule("Email"));

            // Grupo de validaciones para contraseña
            var passwordGroup = new ValidationComposite("Validación de Contraseña");
            passwordGroup.Add(new RequiredFieldRule("Password", "Contraseña"));
            passwordGroup.Add(new PasswordLengthRule(6, "Password"));
            passwordGroup.Add(new PasswordMatchRule("Password", "ConfirmPassword"));

            // Grupo de validaciones para rol
            var roleGroup = new ValidationComposite("Validación de Rol");
            roleGroup.Add(new RequiredFieldRule("Rol", "Rol del usuario"));

            // Agregar todos los grupos al validador principal
            registerValidator.Add(userInfoGroup);
            registerValidator.Add(emailGroup);
            registerValidator.Add(passwordGroup);
            registerValidator.Add(roleGroup);

            return registerValidator;
        }
    }
}
