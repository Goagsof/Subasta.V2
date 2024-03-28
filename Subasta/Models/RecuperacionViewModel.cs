using System.ComponentModel.DataAnnotations;

namespace Subasta.Models
{
    public class RecuperacionViewModel
    {
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido")]
        public string CorreoElectronico { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [DataType(DataType.Password)]
        public string NuevaContraseña { get; set; }

        [Compare("NuevaContraseña", ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        public string ConfirmarContraseña { get; set; }
    }
}
