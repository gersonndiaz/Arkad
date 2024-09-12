using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arkad.Server.Models
{
    [Table("Item")]
    public class Item
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
        [Column("position")]
        public int Position { get; set; }

        [Required]
        [Column("type")]
        public string Type { get; set; }

        [Column("formula")]
        public string Formula { get; set; }

        [Column("formula_aux")]
        public string FormulaAux { get; set; }

        [Required]
        [Column("auto")]
        public bool Auto { get; set; }

        [Required]
        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [Required]
        [Column("modified_date")]
        public DateTime ModifiedDate { get; set; }

        [Required]
        [Column("active")]
        public bool Active { get; set; }

        [Column("group_id")]
        public string GroupId { get; set; }

        #region VIRTUAL
        [ForeignKey("GroupId")]
        public virtual Group Group { get; set; }
        #endregion VIRTUAL
    }
}
