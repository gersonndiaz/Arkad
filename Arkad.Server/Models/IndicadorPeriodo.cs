using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arkad.Server.Models
{
    [Table("IndicadorPeriodo")]
    public class IndicadorPeriodo
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }

        [Column("data")]
        public string Data { get; set; }

        [Column("detalle")]
        public string Detalle { get; set; }

        [Column("b_finalizado")]
        public bool BFinalizado { get; set; }

        [Column("f_creado")]
        public DateTime FCreado { get; set; }

        [Column("f_modificado")]
        public DateTime FModificado { get; set; }

        [Column("b_activo")]
        public bool BActivo { get; set; }

        [Column("reparticion_id")]
        public string ReparticionId { get; set; }

        [Column("periodo_id")]
        public string PeriodoId { get; set; }

        [Column("usuario_id")]
        public string UsuarioId { get; set; }
    }
}
