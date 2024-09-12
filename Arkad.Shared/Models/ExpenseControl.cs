namespace Arkad.Shared.Models
{
    public class ExpenseControl
    {
        public string Id { get; set; }

        public string Data { get; set; }

        public string Explanation { get; set; }

        public bool Finished { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        public bool Active { get; set; }

        public string PeriodId { get; set; }

        public string UserId { get; set; }
    }
}
