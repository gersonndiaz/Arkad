using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arkad.Server.Models
{
    [Table("Reparticion")]
    public class Reparticion
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }
        [Column("codigo")]
        public string Codigo { get; set; }
        [Column("nombre")]
        public string Nombre { get; set; }
        [Column("b_activo")]
        public bool BActivo { get; set; }
    }
}
