using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Subasta.Models
{
    public class LoginAudit
    {
        public string CorreoElectronico { get; set; }
        public DateTime FechaHoraIntento { get; set; }
        public bool Exito { get; set; }
        public string MotivoBloqueo { get; set; }
    }
}