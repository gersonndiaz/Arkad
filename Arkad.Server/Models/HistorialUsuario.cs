using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arkad.Server.Models
{
    [Table("HistorialUsuario")]
    public class HistorialUsuario
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
        [Column("usuario_actual_id")]
        public string UsuarioActualId { get; set; }
        [Column("usuario_id")]
        public string UsuarioId { get; set; }
    }
}
