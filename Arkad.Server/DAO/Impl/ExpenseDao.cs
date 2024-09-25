using Arkad.Server.Models;
using Arkad.Server.Models._Context;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Linq.Expressions;

namespace Arkad.Server.DAO.Impl
{
    public class ExpenseDao : IExpenseDao
    {
        private static string TAG = typeof(ExpenseDao).FullName;
        private Logger logger = LogManager.GetCurrentClassLogger();
        private AppDbContext appDbContext;

        #region Contructor
        /// <summary>
        /// Constructor por defecto, el cual tiene las configuraciones estándar para el acceso a datos
        /// </summary>
        /// <param name="appDbContext"></param>
        public ExpenseDao()
        {
            appDbContext = appDbContext != null ? appDbContext : new AppDbContext();
        }

        /// <summary>
        /// Constructor que recibe el contexto
        /// </summary>
        /// <param name="appDbContext"></param>
        public ExpenseDao(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext != null ? appDbContext : new AppDbContext();
        }
        #endregion Constructor

        #region Expenses
        /// <summary>
        /// Obtiene un gasto por su ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Expense GetById(string id)
        {
            try
            {
                var expense = appDbContext.Expenses
                                            .Where(x => x.Id == id)
                                            .FirstOrDefault();
                return expense;
            }
            catch (Exception ex)
            {
                logger.Error($"[{TAG}] - {ex}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un gasto por el periodo, el item y estado actual
        /// </summary>
        /// <param name="period"></param>
        /// <param name="item"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        public Expense GetByPeriodAndItem(Period period, Item item, bool? active)
        {
            try
            {
                Expression<Func<Expense, bool>> activeExpression;
                if (active.HasValue)
                {
                    activeExpression = x => x.Active == active.Value;
                }
                else
                {
                    activeExpression = x => (x.Active == true || x.Active == false);
                }

                var control = appDbContext.Expenses
                                            .Where(x => x.PeriodId == period.Id)
                                            .Where(x => x.ItemId == item.Id)
                                            .Where(activeExpression)
                                            .OrderBy(x => x.position)
                                            .FirstOrDefault();
                return control;
            }
            catch (Exception ex)
            {
                logger.Error($"[{TAG}] - {ex}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene listado de gastos encontrados
        /// </summary>
        /// <param name="period"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        public List<Expense> GetAll(Period period, bool? active)
        {
            try
            {
                Expression<Func<Expense, bool>> activeExpression;
                if (active.HasValue)
                {
                    activeExpression = x => x.Active == active.Value;
                }
                else
                {
                    activeExpression = x => (x.Active == true || x.Active == false);
                }

                var expenses = appDbContext.Expenses
                                                .Where(x => x.PeriodId == period.Id)
                                                .Where(activeExpression)
                                                .OrderBy(x => x.position)
                                                .ToList();
                return expenses;
            }
            catch (Exception e)
            {
                logger.Error($"[{TAG}] -- {e}");
                throw;
            }
        }
        #endregion Expenses

        #region Control
        /// <summary>
        /// Obtiene un Contorl de Gastos por su ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ExpenseControl GetControlById(string id)
        {
            try
            {
                var control = appDbContext.ExpensesControl
                                            .Where(x => x.Id == id)
                                            .FirstOrDefault();
                return control;
            }
            catch (Exception ex)
            {
                logger.Error($"[{TAG}] - {ex}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un control de gastos por el periodo
        /// </summary>
        /// <param name="period"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        public ExpenseControl GetControlByPeriod(Period period, bool? active)
        {
            try
            {
                Expression<Func<ExpenseControl, bool>> activeExpression;
                if (active.HasValue)
                {
                    activeExpression = x => x.Active == active.Value;
                }
                else
                {
                    activeExpression = x => (x.Active == true || x.Active == false);
                }

                var control = appDbContext.ExpensesControl
                                            .Where(x => x.PeriodId == period.Id)
                                            .Where(activeExpression)
                                            .FirstOrDefault();
                return control;
            }
            catch (Exception ex)
            {
                logger.Error($"[{TAG}] - {ex}");
                throw;
            }
        }

        /// <summary>
        /// Se actualiza la información del control de gastos
        /// </summary>
        /// <param name="control"></param>
        /// <param name="history"></param>
        /// <param name="historyControl"></param>
        /// <returns></returns>
        public bool UpdateControl(ExpenseControl control, History history, HistoryExpenseControl historyControl)
        {
            bool success = false;

            try
            {
                #region TRANSACCION
                using (var dbContextTransaction = appDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        #region Expense Control
                        appDbContext.ExpensesControl.Attach(control);
                        appDbContext.Entry(control).State = EntityState.Modified;
                        success = (appDbContext.SaveChanges() > 0) ? true : false;
                        #endregion Expense Control

                        #region History
                        if (success)
                        {
                            appDbContext.Histories.Attach(history);
                            appDbContext.Entry(history).State = EntityState.Added;
                            success = (appDbContext.SaveChanges() > 0) ? true : false;

                            if (success)
                            {
                                appDbContext.HistoryExpensesControl.Attach(historyControl);
                                appDbContext.Entry(historyControl).State = EntityState.Added;
                                success = (appDbContext.SaveChanges() > 0) ? true : false;
                            }
                        }
                        #endregion History
                    }
                    catch (Exception e)
                    {
                        success = false;
                        logger.Error($"{TAG} -- {e}");
                    }

                    #region COMMIT OR ROLLBACK TRANSACTION
                    if (success)
                    {
                        dbContextTransaction.Commit();
                    }
                    else
                    {
                        dbContextTransaction.Rollback();
                    }
                    #endregion
                }
                #endregion TRANSACCION
            }
            catch (Exception e)
            {
                success = false;
                logger.Error($"{TAG} -- {e}");
            }

            return success;
        }

        /// <summary>
        /// Se actualiza la información del control de gastos
        /// </summary>
        /// <param name="expenses"></param>
        /// <param name="control"></param>
        /// <param name="history"></param>
        /// <param name="historyControl"></param>
        /// <returns></returns>
        public bool UpdateControl(List<Expense> expenses, ExpenseControl control, History history, HistoryExpenseControl historyControl)
        {
            bool success = false;

            try
            {
                #region TRANSACCION
                using (var dbContextTransaction = appDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        #region Expenses
                        if (expenses != null && expenses.Count > 0)
                        {
                            int countErr = 0;
                            foreach (var expense in expenses)
                            {
                                appDbContext.Expenses.Attach(expense);
                                appDbContext.Entry(expense).State = EntityState.Added;
                                countErr += appDbContext.SaveChanges() > 0 ? 0 : 1;
                            }

                            success = countErr > 0 ? false : true;
                        }
                        #endregion Expenses

                        #region Expense Control
                        bool existControl = appDbContext.ExpensesControl.Any(x => x.Id == control.Id);
                        appDbContext.ExpensesControl.Attach(control);
                        appDbContext.Entry(control).State = (existControl) ? EntityState.Modified : EntityState.Added;
                        success = (appDbContext.SaveChanges() > 0) ? true : false;
                        #endregion Expense Control

                        #region History
                        if (success)
                        {
                            appDbContext.Histories.Attach(history);
                            appDbContext.Entry(history).State = EntityState.Added;
                            success = (appDbContext.SaveChanges() > 0) ? true : false;

                            if (success)
                            {
                                appDbContext.HistoryExpensesControl.Attach(historyControl);
                                appDbContext.Entry(historyControl).State = EntityState.Added;
                                success = (appDbContext.SaveChanges() > 0) ? true : false;
                            }
                        }
                        #endregion History
                    }
                    catch (Exception e)
                    {
                        success = false;
                        logger.Error($"{TAG} -- {e}");
                    }

                    #region COMMIT OR ROLLBACK TRANSACTION
                    if (success)
                    {
                        dbContextTransaction.Commit();
                    }
                    else
                    {
                        dbContextTransaction.Rollback();
                    }
                    #endregion
                }
                #endregion TRANSACCION
            }
            catch (Exception e)
            {
                success = false;
                logger.Error($"{TAG} -- {e}");
            }

            return success;
        }
        #endregion Control
    }
}
