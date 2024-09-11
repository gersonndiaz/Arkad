namespace Arkad.Shared.Models
{
    public class HistorialPeriodoAccidentes
    {
        public string Id { get; set; }
        public string Glosa { get; set; }
        public string Descripcion { get; set; }
        public DateTime FCreado { get; set; }
        public bool BActivo { get; set; }
        public string Indicadores { get; set; }
        public string PeriodoId { get; set; }
        public string UsuarioId { get; set; }
    }
}
