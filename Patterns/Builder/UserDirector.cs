using Microsoft.AspNetCore.Identity;

namespace AutoSys.Patterns.Builder
{
    /// <summary>
    /// Director del patrón Builder que orquesta la construcción de usuarios.
    /// Proporciona métodos de alto nivel para crear usuarios con configuraciones específicas.
    /// </summary>
    public class UserDirector
    {
        private readonly IUserBuilder _builder;

        public UserDirector(IUserBuilder builder)
        {
            _builder = builder;
        }

        /// <summary>
        /// Construye un usuario estándar con todos los datos básicos
        /// </summary>
        public async Task<UserBuildResult> BuildStandardUserAsync(
            string username, 
            string email, 
            string password, 
            string role)
        {
            _builder.Reset();
            
            return await _builder
                .SetUsername(username)
                .SetEmail(email)
                .SetPassword(password)
                .SetRole(role)
                .BuildAsync();
        }

        /// <summary>
        /// Construye un usuario administrador
        /// </summary>
        public async Task<UserBuildResult> BuildAdminUserAsync(
            string username, 
            string email, 
            string password)
        {
            return await BuildStandardUserAsync(username, email, password, "Administrador");
        }

        /// <summary>
        /// Construye un usuario con rol de recepcionista
        /// </summary>
        public async Task<UserBuildResult> BuildReceptionistUserAsync(
            string username, 
            string email, 
            string password)
        {
            return await BuildStandardUserAsync(username, email, password, "Recepcionista");
        }

        /// <summary>
        /// Construye un usuario con rol de mecánico
        /// </summary>
        public async Task<UserBuildResult> BuildMechanicUserAsync(
            string username, 
            string email, 
            string password)
        {
            return await BuildStandardUserAsync(username, email, password, "Mecanico");
        }
    }
}
