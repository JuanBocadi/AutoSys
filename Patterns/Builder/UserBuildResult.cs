using Microsoft.AspNetCore.Identity;

namespace AutoSys.Patterns.Builder
{
    /// <summary>
    /// Resultado del proceso de construcción de usuario
    /// </summary>
    public class UserBuildResult
    {
        public bool Succeeded { get; private set; }
        public IdentityUser? User { get; private set; }
        public List<string> Errors { get; private set; } = new();

        private UserBuildResult() { }

        public static UserBuildResult Success(IdentityUser user)
        {
            return new UserBuildResult
            {
                Succeeded = true,
                User = user
            };
        }

        public static UserBuildResult Failure(IEnumerable<string> errors)
        {
            return new UserBuildResult
            {
                Succeeded = false,
                Errors = errors.ToList()
            };
        }

        public static UserBuildResult Failure(string error)
        {
            return new UserBuildResult
            {
                Succeeded = false,
                Errors = new List<string> { error }
            };
        }
    }
}
