using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arkad.Server.Models
{
    [Table("Group")]
    public class Group
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Required]
        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [Required]
        [Column("modified_date")]
        public DateTime ModifiedDate { get; set; }

        [Required]
        [Column("active")]
        public bool Active { get; set; }
    }
}
