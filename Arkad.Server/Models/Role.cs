using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arkad.Server.Models
{
    [Table("Role")]
    public class Role
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }
        [Column("code")]
        public string Code { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("created_date")]
        public DateTime CreatedDate { get; set; }
        [Column("modified_date")]
        public DateTime ModifiedDate { get; set; }
        [Column("active")]
        public bool Active { get; set; }
    }
}
