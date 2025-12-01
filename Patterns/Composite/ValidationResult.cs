namespace AutoSys.Patterns.Composite
{
    /// <summary>
    /// Resultado de una operación de validación.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; private set; }
        public List<string> Errors { get; private set; } = new();

        private ValidationResult() { }

        public static ValidationResult Success() => new() { IsValid = true };
        
        public static ValidationResult Failure(string error) => new() 
        { 
            IsValid = false, 
            Errors = new List<string> { error } 
        };

        public static ValidationResult Failure(List<string> errors) => new() 
        { 
            IsValid = false, 
            Errors = errors 
        };

        /// <summary>
        /// Combina múltiples resultados de validación
        /// </summary>
        public static ValidationResult Combine(IEnumerable<ValidationResult> results)
        {
            var allErrors = new List<string>();
            foreach (var result in results)
            {
                if (!result.IsValid)
                {
                    allErrors.AddRange(result.Errors);
                }
            }

            return allErrors.Count == 0 
                ? Success() 
                : Failure(allErrors);
        }
    }
}
