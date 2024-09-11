using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arkad.Server.Models
{
    [Table("HistorialReparticion")]
    public class HistorialReparticion
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
        [Column("reparticion_id")]
        public string ReparticionId { get; set; }
        [Column("usuario_id")]
        public string UsuarioId { get; set; }
    }
}
