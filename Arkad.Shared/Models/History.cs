namespace Arkad.Shared.Models
{
    public class History
    {
        public string Id { get; set; }

        public string Explanation { get; set; }

        public string Description { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        public bool Active { get; set; }
    }
}
