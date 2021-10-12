using System;

namespace NtxWsDolar.Classes
{

    public class ScraperDolar
    {
        public int Id { get; set; }
        public DateTime FechaPagina  { get; set; }
        public DateTime FechaProcesado { get; set; }
        public string StrFechaPagina { get; set; }
        public string StrFechaProcesado { get; set; }
        public decimal CambioDolar { get; set; }
        public string ErrorDescripcion { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

}
