using Microsoft.AspNetCore.Identity;

namespace AutoSys.Patterns.Strategy
{
    /// <summary>
    /// Resultado de una operación de autenticación
    /// </summary>
    public class AuthenticationResult
    {
        public bool IsSuccess { get; private set; }
        public string? ErrorMessage { get; private set; }
        public IdentityUser? User { get; private set; }
        public string? UsedStrategy { get; private set; }

        private AuthenticationResult() { }

        public static AuthenticationResult Successful(IdentityUser user, string strategy)
        {
            return new AuthenticationResult
            {
                IsSuccess = true,
                User = user,
                UsedStrategy = strategy
            };
        }

        public static AuthenticationResult Failed(string errorMessage)
        {
            return new AuthenticationResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
