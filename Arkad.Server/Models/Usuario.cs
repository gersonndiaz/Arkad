using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arkad.Server.Models
{
    [Table("Usuario")]
    public class Usuario
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }
        [Column("run")]
        public string Run { get; set; }
        [Column("nombre")]
        public string Nombre { get; set; }
        [Column("correo")]
        [DataType(DataType.EmailAddress)]
        public string Correo { get; set; }
        [Column("clave")]
        public string Clave { get; set; }
        [Column("f_ultima_conexion")]
        public DateTime? FUltimaConexion { get; set; }
        [Column("f_creado")]
        public DateTime FCreado { get; set; }
        [Column("f_modificado")]
        public DateTime FModificado { get; set; }
        [Column("b_activo")]
        public bool BActivo { get; set; }

        [Column("rol_id")]
        public string RolId { get; set; }
    }
}
