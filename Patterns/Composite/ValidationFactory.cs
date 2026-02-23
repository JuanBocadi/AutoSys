namespace AutoSys.Patterns.Composite
{
    /// Crea combinaciones predefinidas de validaciones para login y registro
    public static class ValidationFactory
    {
        public static IValidationComponent CreateLoginValidation()
        {
            var composite = new ValidationComposite();
            composite.Add(new UsernameRequiredValidation());
            composite.Add(new PasswordLengthValidation(6));
            return composite;
        }

        public static IValidationComponent CreateRegisterValidation()
        {
            var composite = new ValidationComposite();
            composite.Add(new UsernameRequiredValidation());
            composite.Add(new EmailFormatValidation());
            composite.Add(new PasswordLengthValidation(6));
            composite.Add(new PasswordComplexityValidation());
            composite.Add(new RoleRequiredValidation());
            return composite;
        }
    }
}
