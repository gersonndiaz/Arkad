using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Arkad.Server.Models
{
    [Table("HistoryGroup")]
    public class HistoryGroup
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }

        [Column("data")]
        public string Data { get; set; }

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
        [Column("history_id")]
        public string HistoryId { get; set; }

        [Required]
        [Column("group_id")]
        public string GroupId { get; set; }

        [Required]
        [Column("user_id")]
        public string UserId { get; set; }

        #region VIRTUAL
        [ForeignKey("HistoryId")]
        public virtual History History { get; set; }

        [ForeignKey("GroupId")]
        public virtual Group Group { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        #endregion VIRTUAL
    }
}
