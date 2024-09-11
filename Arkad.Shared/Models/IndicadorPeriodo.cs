namespace Arkad.Shared.Models
{
    public class IndicadorPeriodo
    {
        public string Id { get; set; }
        public string Data { get; set; }
        public string Detalle { get; set; }
        public bool BFinalizado { get; set; }
        public DateTime FCreado { get; set; }
        public DateTime FModificado { get; set; }
        public bool BActivo { get; set; }
        public string ReparticionId { get; set; }
        public string PeriodoId { get; set; }
        public string UsuarioId { get; set; }
    }
}
