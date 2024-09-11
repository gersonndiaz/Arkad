using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arkad.Server.Models
{
    [Table("Accidente")]
    public class Accidente
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }
        [Column("valor")]
        public float Valor { get; set; }
        [Column("tipo")]
        public string Tipo { get; set; }
        [Column("reparticion_id")]
        public string ReparticionId { get; set; }
        [Column("periodo_id")]
        public string PeriodoId { get; set; }
        [Column("item_id")]
        public string ItemId { get; set; }
        [Column("usuario_id")]
        public string UsuarioId { get; set; }
        [Column("f_creado")]
        public DateTime FCreado { get; set; }
        [Column("b_activo")]
        public bool BActivo { get; set; }
    }
}
