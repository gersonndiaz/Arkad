using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Arkad.Server.Models
{
    [Table("ExpenseControl")]
    public class ExpenseControl
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }

        [Required]
        [Column("data")]
        public string Data { get; set; }

        [Column("explanation")]
        public string Explanation { get; set; }

        [Required]
        [Column("finished")]
        public bool Finished { get; set; }

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
        [Column("period_id")]
        public string PeriodId { get; set; }

        [Required]
        [Column("user_id")]
        public string UserId { get; set; }

        #region VIRTUAL
        [ForeignKey("PeriodId")]
        public virtual Period Period { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        #endregion VIRTUAL
    }
}
