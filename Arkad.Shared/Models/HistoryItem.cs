namespace Arkad.Shared.Models
{
    public class HistoryItem
    {
        public string Id { get; set; }

        public string Data { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        public bool Active { get; set; }

        public string HistoryId { get; set; }

        public string ItemId { get; set; }

        public string UserId { get; set; }
    }
}
