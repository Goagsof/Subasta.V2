using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Subasta.Models
{
    public class LibroV
    {
        public string connectionString = "Server=GABS;Database=Subasta;User Id=sa;Password=123456789;";

        public string GetConnectionString()
        {
            return connectionString;
        }
    }
}
