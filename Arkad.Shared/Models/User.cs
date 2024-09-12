namespace Arkad.Shared.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime? LastLogin { get; set; }
        public string LastLoginIP { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool Active { get; set; }
        public string RoleId { get; set; }
    }
}
