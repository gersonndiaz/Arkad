namespace Arkad.Shared.Models
{
    public class Period
    {
        public string Id { get; set; }

        public int Year { get; set; }

        public int Month { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        public bool Current { get; set; }

        public bool Active { get; set; }
    }
}
