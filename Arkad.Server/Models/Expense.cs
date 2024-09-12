using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Arkad.Server.Models
{
    [Table("Expense")]
    public class Expense
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }

        [Required]
        [Column("value")]
        public double Value { get; set; }

        [Column("explanation")]
        public string Explanation { get; set; }

        [Required]
        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [Required]
        [Column("modified_date")]
        public DateTime ModifiedDate { get; set; }

        [Required]
        [Column("active")]
        public bool Active { get; set; }

        [Required]
        [Column("item_id")]
        public string ItemId { get; set; }

        [Required]
        [Column("period_id")]
        public string PeriodId { get; set; }

        [Required]
        [Column("user_id")]
        public string UserId { get; set; }

        #region VIRTUAL
        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; }

        [ForeignKey("PeriodId")]
        public virtual Period Period { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        #endregion VIRTUAL
    }
}
