using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Subasta.Models
{
    public class GanarViewModel
    {
        public int GanadorSubastaID { get; set; } 
        public int SubastaID { get; set; }
        public string Titulo { get; set; }
        public decimal Precio { get; set; }
        public DateTime FechaGanado { get; set; }
        public string Usuario { get; set; }
        public string ImagenProducto { get; set; }
    }


}