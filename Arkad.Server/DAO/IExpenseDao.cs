using Arkad.Server.Models;

namespace Arkad.Server.DAO
{
    interface IExpenseDao
    {
        #region Expenses
        List<Expense> GetAll(Period period, bool? active);
        #endregion Expenses

        #region Control
        ExpenseControl GetControlByPeriod(Period period, bool? active);
        #endregion Control
    }
}
