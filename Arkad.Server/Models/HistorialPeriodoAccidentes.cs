using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Arkad.Server.Models
{
    [Table("HistorialPeriodoAccidentes")]
    public class HistorialPeriodoAccidentes
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }
        [Column("glosa")]
        public string Glosa { get; set; }
        [Column("descripcion")]
        public string Descripcion { get; set; }
        [Column("f_creado")]
        public DateTime FCreado { get; set; }
        [Column("b_activo")]
        public bool BActivo { get; set; }
        [Column("indicadores")]
        public string Indicadores { get; set; }
        [Column("periodo_id")]
        public string PeriodoId { get; set; }
        [Column("usuario_id")]
        public string UsuarioId { get; set; }
    }
}
