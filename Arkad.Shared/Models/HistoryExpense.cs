﻿namespace Arkad.Shared.Models
{
    public class HistoryExpense
    {
        public string Id { get; set; }

        public string Data { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        public bool Active { get; set; }

        public string HistoryId { get; set; }

        public string ExpenseId { get; set; }

        public string UserId { get; set; }
    }
}
