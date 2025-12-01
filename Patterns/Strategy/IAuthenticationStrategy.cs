using Microsoft.AspNetCore.Identity;

namespace AutoSys.Patterns.Strategy
{
    /// <summary>
    /// Interface del patrón Strategy para estrategias de autenticación.
    /// Permite implementar diferentes formas de autenticar usuarios.
    /// </summary>
    public interface IAuthenticationStrategy
    {
        /// <summary>
        /// Nombre descriptivo de la estrategia
        /// </summary>
        string StrategyName { get; }

        /// <summary>
        /// Intenta encontrar al usuario usando el identificador proporcionado
        /// </summary>
        /// <param name="identifier">Email o nombre de usuario</param>
        /// <returns>Usuario encontrado o null</returns>
        Task<IdentityUser?> FindUserAsync(string identifier);
    }
}
