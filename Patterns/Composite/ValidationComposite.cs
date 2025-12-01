namespace AutoSys.Patterns.Composite
{
    /// <summary>
    /// Patrón Composite: Composite - Agrupa múltiples validaciones
    /// Permite tratar un grupo de validaciones como una sola validación
    /// </summary>
    public class ValidationComposite : IValidationComponent
    {
        private readonly List<IValidationComponent> _validations = new();
        private string _errorMessage = string.Empty;

        public void Add(IValidationComponent validation)
        {
            _validations.Add(validation);
        }

        public void Remove(IValidationComponent validation)
        {
            _validations.Remove(validation);
        }

        public bool Validate(ValidationContext context)
        {
            foreach (var validation in _validations)
            {
                if (!validation.Validate(context))
                {
                    _errorMessage = validation.GetErrorMessage();
                    return false;
                }
            }
            return true;
        }

        public string GetErrorMessage() => _errorMessage;
    }
}
