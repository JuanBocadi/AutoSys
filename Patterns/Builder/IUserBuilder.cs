using Microsoft.AspNetCore.Identity;

namespace AutoSys.Patterns.Builder
{
    /// <summary>
    /// Interface del patrón Builder para construir usuarios.
    /// Define los pasos necesarios para crear un usuario completo.
    /// </summary>
    public interface IUserBuilder
    {
        /// <summary>
        /// Establece el nombre de usuario
        /// </summary>
        IUserBuilder SetUsername(string username);

        /// <summary>
        /// Establece el correo electrónico
        /// </summary>
        IUserBuilder SetEmail(string email);

        /// <summary>
        /// Establece la contraseña
        /// </summary>
        IUserBuilder SetPassword(string password);

        /// <summary>
        /// Establece el rol del usuario
        /// </summary>
        IUserBuilder SetRole(string role);

        /// <summary>
        /// Construye y retorna el resultado del proceso de creación
        /// </summary>
        Task<UserBuildResult> BuildAsync();

        /// <summary>
        /// Reinicia el builder para crear un nuevo usuario
        /// </summary>
        void Reset();
    }
}
