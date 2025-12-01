using Microsoft.AspNetCore.Identity;

namespace AutoSys.Patterns.Strategy
{
    /// <summary>
    /// Estrategia de autenticación por nombre de usuario (Strategy Pattern)
    /// </summary>
    public class UsernameAuthenticationStrategy : IAuthenticationStrategy
    {
        private readonly UserManager<IdentityUser> _userManager;

        public string StrategyName => "Autenticación por Nombre de Usuario";

        public UsernameAuthenticationStrategy(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityUser?> FindUserAsync(string identifier)
        {
            return await _userManager.FindByNameAsync(identifier);
        }
    }
}
