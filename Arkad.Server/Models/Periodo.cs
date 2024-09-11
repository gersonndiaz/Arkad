using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arkad.Server.Models
{
    [Table("periodo")]
    public class Periodo
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }

        [Column("anio")]
        public int Anio { get; set; }

        [Column("mes")]
        public int Mes { get; set; }

        [Column("f_creado")]
        public DateTime FCreado { get; set; }

        [Column("b_actual")]
        public bool BActual { get; set; }

        [Column("b_activo")]
        public bool BActivo { get; set; }
    }
}
