using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arkad.Server.Models
{
    [Table("HistorialItem")]
    public class HistorialItem
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
        [Column("item_id")]
        public string ItemId { get; set; }
        [Column("usuario_id")]
        public string UsuarioId { get; set; }
    }
}
