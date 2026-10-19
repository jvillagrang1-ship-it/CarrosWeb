using System.ComponentModel.DataAnnotations;

namespace CarrosWeb.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Ingresa tu correo.")]
        [Display(Name = "Correo")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "Ingresa tu contraseña.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; }
    }

    public class CrearAdminInicialViewModel
    {
        [Required(ErrorMessage = "Ingresa tu nombre.")]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Ingresa un correo.")]
        [Display(Name = "Correo")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "Ingresa una contraseña.")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "La contraseña debe tener al menos 4 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; }
    }
}