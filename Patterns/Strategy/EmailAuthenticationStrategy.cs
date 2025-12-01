using Microsoft.AspNetCore.Identity;

namespace AutoSys.Patterns.Strategy
{
    /// <summary>
    /// Estrategia de autenticación por correo electrónico (Strategy Pattern)
    /// </summary>
    public class EmailAuthenticationStrategy : IAuthenticationStrategy
    {
        private readonly UserManager<IdentityUser> _userManager;

        public string StrategyName => "Autenticación por Email";

        public EmailAuthenticationStrategy(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityUser?> FindUserAsync(string identifier)
        {
            return await _userManager.FindByEmailAsync(identifier);
        }
    }
}
