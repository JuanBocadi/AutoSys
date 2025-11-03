using System.ComponentModel.DataAnnotations;

namespace AutoSys.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre de usuario no puede exceder los 50 caracteres.")]
        [Display(Name = "Nombre de Usuario")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress]
        [Display(Name = "Correo Electrónico")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public required string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
    public required string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un rol.")]
        [Display(Name = "Rol del usuario")]
    public required string Rol { get; set; }
    }
}
