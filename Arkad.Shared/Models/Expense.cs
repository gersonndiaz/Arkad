namespace Arkad.Shared.Models
{
    public class Expense
    {
        public string Id { get; set; }

        public double Value { get; set; }

        public string Explanation { get; set; }

        public int position { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        public bool Active { get; set; }

        public string ItemId { get; set; }

        public string PeriodId { get; set; }

        public string UserId { get; set; }

        public Item Item { get; set; }
    }
}
