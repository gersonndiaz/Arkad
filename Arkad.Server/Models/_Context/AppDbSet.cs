using Microsoft.EntityFrameworkCore;

namespace Arkad.Server.Models._Context
{
    public class AppDbSet : DbContext
    {
        public virtual DbSet<Expense> Expenses { get; set; }
        public virtual DbSet<ExpenseControl> ExpensesControl { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<History> Histories { get; set; }
        public virtual DbSet<HistoryExpense> HistoryExpenses { get; set; }
        public virtual DbSet<HistoryExpenseControl> HistoryExpensesControl { get; set; }
        public virtual DbSet<HistoryGroup> HistoryGroups { get; set; }
        public virtual DbSet<HistoryItem> HistoryItems { get; set; }
        public virtual DbSet<HistoryPeriod> HistoryPeriods { get; set; }
        public virtual DbSet<HistoryUser> HistoryUsers { get; set; }
        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<Period> Periods { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<User> Users { get; set; }
    }
}
