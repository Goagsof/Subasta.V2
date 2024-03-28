using System.Collections.Generic;

namespace Subasta.Models
{
    public class PreguntaViewModel
    {
        public List<PreguntaSeguridad> Preguntas { get; set; }
    }

    public class PreguntaSeguridad
    {
        public int PreguntaID { get; set; }
        public string Pregunta { get; set; }
    }
}
