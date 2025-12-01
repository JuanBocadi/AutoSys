using Microsoft.AspNetCore.Identity;

namespace AutoSys.Patterns.Builder
{
    /// <summary>
    /// Implementación concreta del patrón Builder para crear usuarios de Identity.
    /// Permite construir usuarios paso a paso con una interfaz fluida.
    /// </summary>
    public class IdentityUserBuilder : IUserBuilder
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        
        private string _username = string.Empty;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _role = string.Empty;

        public IdentityUserBuilder(
            UserManager<IdentityUser> userManager, 
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IUserBuilder SetUsername(string username)
        {
            _username = username;
            return this;
        }

        public IUserBuilder SetEmail(string email)
        {
            _email = email;
            return this;
        }

        public IUserBuilder SetPassword(string password)
        {
            _password = password;
            return this;
        }

        public IUserBuilder SetRole(string role)
        {
            _role = role;
            return this;
        }

        public async Task<UserBuildResult> BuildAsync()
        {
            // Crear el usuario de Identity
            var user = new IdentityUser 
            { 
                UserName = _username, 
                Email = _email 
            };

            // Intentar crear el usuario con la contraseña
            var createResult = await _userManager.CreateAsync(user, _password);

            if (!createResult.Succeeded)
            {
                return UserBuildResult.Failure(
                    createResult.Errors.Select(e => e.Description));
            }

            // Asignar rol si existe
            if (!string.IsNullOrEmpty(_role))
            {
                if (await _roleManager.RoleExistsAsync(_role))
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, _role);
                    if (!roleResult.Succeeded)
                    {
                        // Si falla la asignación de rol, eliminar el usuario creado
                        await _userManager.DeleteAsync(user);
                        return UserBuildResult.Failure(
                            roleResult.Errors.Select(e => e.Description));
                    }
                }
                else
                {
                    await _userManager.DeleteAsync(user);
                    return UserBuildResult.Failure($"El rol '{_role}' no existe.");
                }
            }

            return UserBuildResult.Success(user);
        }

        public void Reset()
        {
            _username = string.Empty;
            _email = string.Empty;
            _password = string.Empty;
            _role = string.Empty;
        }
    }
}
