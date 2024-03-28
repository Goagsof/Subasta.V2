using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Subasta.Models
{
    public class PerfilViewModel
    {
        public int UserID { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string CorreoElectronico { get; set; }
        public string Contraseña { get; set; }
        public string FotoPerfil { get; set; }
    }
}