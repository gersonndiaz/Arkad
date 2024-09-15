using Arkad.Server.Models;
using Arkad.Server.Models._Context;
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
        #endregion Control
    }
}
