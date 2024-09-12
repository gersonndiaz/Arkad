namespace Arkad.Shared.Models
{
    public class HistoryPeriod
    {
        public string Id { get; set; }

        public string Data { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        public bool Active { get; set; }

        public string HistoryId { get; set; }

        public string PeriodId { get; set; }

        public string UserId { get; set; }


        public string UserName { get; set; }

        public History History { get; set; }
    }
}
