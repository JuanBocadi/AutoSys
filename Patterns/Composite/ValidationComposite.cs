namespace AutoSys.Patterns.Composite
{
    /// <summary>
    /// Composite que agrupa múltiples validaciones.
    /// Implementa el patrón Composite permitiendo agregar validaciones hijas.
    /// </summary>
    public class ValidationComposite : IValidationComponent
    {
        private readonly List<IValidationComponent> _children = new();
        
        public string Name { get; }

        public ValidationComposite(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Agrega un componente de validación al composite
        /// </summary>
        public void Add(IValidationComponent component)
        {
            _children.Add(component);
        }

        /// <summary>
        /// Remueve un componente de validación del composite
        /// </summary>
        public void Remove(IValidationComponent component)
        {
            _children.Remove(component);
        }

        /// <summary>
        /// Obtiene todos los componentes hijos
        /// </summary>
        public IReadOnlyList<IValidationComponent> GetChildren() => _children.AsReadOnly();

        /// <summary>
        /// Ejecuta todas las validaciones hijas y combina los resultados
        /// </summary>
        public ValidationResult Validate(Dictionary<string, object?> context)
        {
            var results = new List<ValidationResult>();
            
            foreach (var child in _children)
            {
                results.Add(child.Validate(context));
            }

            return ValidationResult.Combine(results);
        }
    }
}
