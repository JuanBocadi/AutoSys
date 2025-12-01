namespace AutoSys.Patterns.Composite
{
    /// <summary>
    /// Interface base del patrón Composite para validación.
    /// Define el componente que puede ser una hoja (regla simple) o un composite (grupo de reglas).
    /// </summary>
    public interface IValidationComponent
    {
        /// <summary>
        /// Nombre descriptivo de la validación
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Ejecuta la validación y retorna el resultado
        /// </summary>
        /// <param name="context">Diccionario con los datos a validar</param>
        /// <returns>Resultado de la validación</returns>
        ValidationResult Validate(Dictionary<string, object?> context);
    }
}
