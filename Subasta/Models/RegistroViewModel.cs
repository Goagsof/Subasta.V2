using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Subasta.Models
{
    public class RegistroViewModel
    {
        [Required(ErrorMessage = "El campo Nombre es requerido")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El campo Apellido es requerido")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El campo Correo Electrónico es requerido")]
        [EmailAddress(ErrorMessage = "El formato del Correo Electrónico no es válido")]
        public string CorreoElectronico { get; set; }

        [Required(ErrorMessage = "El campo Contraseña es requerido")]
        [DataType(DataType.Password)]
        public string Contraseña { get; set; }

        [DataType(DataType.Password)]
        [Compare("Contraseña", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarContraseña { get; set; }

        [Required(ErrorMessage = "Selecciona una pregunta de seguridad")]
        public int PreguntaID1 { get; set; }

        [Required(ErrorMessage = "El campo Respuesta 1 es requerido")]
        public string Respuesta1 { get; set; }

        [Required(ErrorMessage = "Selecciona una pregunta de seguridad")]
        public int PreguntaID2 { get; set; }

        [Required(ErrorMessage = "El campo Respuesta 2 es requerido")]
        public string Respuesta2 { get; set; }

        [Required(ErrorMessage = "Selecciona una pregunta de seguridad")]
        public int PreguntaID3 { get; set; }

        [Required(ErrorMessage = "El campo Respuesta 3 es requerido")]
        public string Respuesta3 { get; set; }

        [Required(ErrorMessage = "El campo Provincia es requerido")]
        public string Provincia { get; set; }

        [Required(ErrorMessage = "El campo Cantón es requerido")]
        public string Canton { get; set; }

        [Required(ErrorMessage = "El campo Distrito es requerido")]
        public string Distrito { get; set; }

        public List<PreguntaSeguridad> PreguntasDeSeguridad { get; set; }
    }
}
