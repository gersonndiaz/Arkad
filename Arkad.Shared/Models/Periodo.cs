namespace Arkad.Shared.Models
{
    public class Periodo
    {
        public string Id { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
        public DateTime FCreado { get; set; }
        public bool BActual { get; set; }
        public bool BActivo { get; set; }
    }
}
