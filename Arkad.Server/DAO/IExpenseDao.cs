using Arkad.Server.Models;

namespace Arkad.Server.DAO
{
    interface IExpenseDao
    {
        #region Expenses
        Expense GetById(string id);
        Expense GetByPeriodAndItem(Period period, Item item, bool? active);
        List<Expense> GetAll(Period period, bool? active);
        #endregion Expenses

        #region Control
        ExpenseControl GetControlById(string id);
        ExpenseControl GetControlByPeriod(Period period, bool? active);
        List<ExpenseControl> GetControlAllByMonths(int months, bool? active);
        bool UpdateControl(ExpenseControl control, History history, HistoryExpenseControl historyControl);
        bool UpdateControl(List<Expense> expenses, ExpenseControl control, History history, HistoryExpenseControl historyControl);
        #endregion Control
    }
}
