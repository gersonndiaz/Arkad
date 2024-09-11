using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Arkad.Server.Models
{
    [Table("Item")]
    public class Item
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }

        [Required]
        [Column("nombre")]
        public string Nombre { get; set; }

        [Column("descripcion")]
        public string Descripcion { get; set; }

        [Required]
        [Column("b_manual")]
        public bool BManual { get; set; } // Mapeo a un entero (0 o 1)

        [Required]
        [Column("posicion")]
        public int Posicion { get; set; }

        [Required]
        [Column("tipo")]
        public string Tipo { get; set; }

        [Column("formula")]
        public string Formula { get; set; }

        [Column("formula_aux")]
        public string FormulaAux { get; set; }

        [Required]
        [Column("f_creado")]
        public DateTime FCreado { get; set; } // Convertido a DateTime para facilidad en C#

        [Column("b_mandante")]
        public bool? BMandante { get; set; } // Mapeo a un entero (0 o 1)

        [Column("b_contratista")]
        public bool? BContratista { get; set; } // Mapeo a un entero (0 o 1)

        [Required]
        [Column("b_activo")]
        public bool BActivo { get; set; } // Mapeo a un entero (0 o 1)
    }
}
