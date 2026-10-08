using System.ComponentModel.DataAnnotations;

namespace ResilienciaNorte.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo electrónico o DNI es requerido.")]
        public string EmailODni { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool Recordarme { get; set; }
    }

    public class RegistroCiudadanoViewModel
    {
        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "El DNI debe tener 8 dígitos.")]
        [RegularExpression("^[0-9]{8}$", ErrorMessage = "Solo se permiten 8 dígitos numéricos.")]
        public string Dni { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(120)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono celular es obligatorio.")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número celular debe tener 9 dígitos.")]
        [RegularExpression("^9[0-9]{8}$", ErrorMessage = "Debe ser un número celular que empiece con 9.")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Mínimo 6 caracteres.")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}