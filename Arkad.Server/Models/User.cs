using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arkad.Server.Models
{
    [Table("User")]
    public class User
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("last_login")]
        public DateTime? LastLogin { get; set; }

        [Column("last_login_ip")]
        public string LastLoginIP { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [Column("modified_date")]
        public DateTime ModifiedDate { get; set; }

        [Column("active")]
        public bool Active { get; set; }

        [Column("role_id")]
        public string RoleId { get; set; }

        #region VIRTUAL
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }
        #endregion VIRTUAL
    }
}
