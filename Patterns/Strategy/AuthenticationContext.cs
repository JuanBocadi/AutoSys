using Microsoft.AspNetCore.Identity;

namespace AutoSys.Patterns.Strategy
{
    /// <summary>
    /// Contexto que utiliza las estrategias de autenticación.
    /// Implementa el patrón Strategy permitiendo cambiar dinámicamente la forma de autenticación.
    /// </summary>
    public class AuthenticationContext
    {
        private readonly List<IAuthenticationStrategy> _strategies;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AuthenticationContext(
            SignInManager<IdentityUser> signInManager,
            IEnumerable<IAuthenticationStrategy> strategies)
        {
            _signInManager = signInManager;
            _strategies = strategies.ToList();
        }

        /// <summary>
        /// Agrega una estrategia de autenticación
        /// </summary>
        public void AddStrategy(IAuthenticationStrategy strategy)
        {
            _strategies.Add(strategy);
        }

        /// <summary>
        /// Intenta autenticar al usuario probando todas las estrategias disponibles
        /// </summary>
        /// <param name="identifier">Email o nombre de usuario</param>
        /// <param name="password">Contraseña</param>
        /// <returns>Resultado de autenticación con información del usuario</returns>
        public async Task<AuthenticationResult> AuthenticateAsync(string identifier, string password)
        {
            IdentityUser? user = null;
            string? usedStrategy = null;

            // Intentar encontrar al usuario con cada estrategia
            foreach (var strategy in _strategies)
            {
                user = await strategy.FindUserAsync(identifier);
                if (user != null)
                {
                    usedStrategy = strategy.StrategyName;
                    break;
                }
            }

            if (user == null)
            {
                return AuthenticationResult.Failed("Usuario no encontrado.");
            }

            // Intentar iniciar sesión
            var signInResult = await _signInManager.PasswordSignInAsync(
                user.UserName!, 
                password, 
                isPersistent: false, 
                lockoutOnFailure: false);

            if (signInResult.Succeeded)
            {
                return AuthenticationResult.Successful(user, usedStrategy ?? "Desconocido");
            }

            if (signInResult.IsLockedOut)
            {
                return AuthenticationResult.Failed("La cuenta está bloqueada. Intente más tarde.");
            }

            return AuthenticationResult.Failed("Contraseña incorrecta.");
        }
    }
}
