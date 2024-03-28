using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Subasta.Models
{
    public class UserInfoViewModel
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string CorreoElectronico { get; set; }
        public UserInfoViewModel UserInfo { get; set; }

        public decimal TotalPagar { get; set; }
    }
}