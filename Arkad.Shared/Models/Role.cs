namespace Arkad.Shared.Models
{
    public class Role
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool Active { get; set; }
    }
}
