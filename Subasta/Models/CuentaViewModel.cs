using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Subasta.Models
{
        public class CuentaViewModel
        {
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public string CorreoElectronico { get; set; }
            public List<SubastaViewModel> SubastasPublicadas { get; set; }
        }

        public class SubastaViewModel
        {
            public int SubastaID { get; set; }
            public string Titulo { get; set; }
            public string Descripcion { get; set; }
            public decimal PrecioActual { get; set; }
            public DateTime FechaInicio { get; set; }
            public DateTime FechaFin { get; set; }
            public string Estado { get; set; }
            public string ImagenProducto { get; set; }
        }
}


