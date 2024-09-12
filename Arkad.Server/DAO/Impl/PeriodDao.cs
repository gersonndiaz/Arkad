using Arkad.Server.Models;
using Arkad.Server.Models._Context;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Linq.Expressions;

namespace Arkad.Server.DAO.Impl
{
    public class PeriodDao : IPeriodDao
    {
        private static string TAG = typeof(PeriodDao).FullName;
        private Logger logger = LogManager.GetCurrentClassLogger();
        private AppDbContext appDbContext;

        #region Contructor
        /// <summary>
        /// Constructor por defecto, el cual tiene las configuraciones estándar para el acceso a datos
        /// </summary>
        /// <param name="appDbContext"></param>
        public PeriodDao()
        {
            this.appDbContext = (appDbContext != null) ? appDbContext : new AppDbContext();
        }

        /// <summary>
        /// Constructor que recibe el contexto
        /// </summary>
        /// <param name="appDbContext"></param>
        public PeriodDao(AppDbContext appDbContext)
        {
            this.appDbContext = (appDbContext != null) ? appDbContext : new AppDbContext();
        }
        #endregion Constructor

        #region Period
        /// <summary>
        /// Obtiene un period por su ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Period GetById(string id)
        {
            try
            {
                var period = appDbContext.Periods
                                    .Where(x => x.Id == id)
                                    .FirstOrDefault();
                return period;
            }
            catch (Exception ex)
            {
                logger.Error($"[{TAG}] - {ex}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene el period active actual
        /// </summary>
        /// <returns></returns>
        public Period GetCurrent()
        {
            try
            {
                var period = appDbContext.Periods
                                    .Where(x => x.Current == true)
                                    .FirstOrDefault();
                return period;
            }
            catch (Exception ex)
            {
                logger.Error($"[{TAG}] - {ex}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un period especifico 
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public Period GetByYearMonth(int year, int month)
        {
            try
            {
                var period = appDbContext.Periods
                                    .Where(x => x.Year == year)
                                    .Where(x => x.Month == month)
                                    .FirstOrDefault();
                return period;
            }
            catch (Exception ex)
            {
                logger.Error($"[{TAG}] - {ex}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene listado de Periods encontrados
        /// </summary>
        /// <param name="active"></param>
        /// <returns></returns>
        public List<Period> GetAll(bool? active)
        {
            try
            {
                #region Expresión Activo
                Expression<Func<Period, bool>> activeExpression;
                if (active.HasValue)
                {
                    activeExpression = x => x.Active == active.Value;
                }
                else
                {
                    activeExpression = x => x.Active == true || x.Active == false;
                }
                #endregion Expresión Activo

                var list = appDbContext.Periods
                                    .Where(activeExpression)
                                    .OrderByDescending(x => x.Year)
                                    .ThenByDescending(x => x.Month)
                                    .ToList();
                return list;
            }
            catch (Exception ex)
            {
                logger.Error($"[{TAG}] - {ex}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene listado de Periods encontrados
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="search"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        public List<Period> GetAll(int limit, int offset, string search, bool? active)
        {
            try
            {
                //#region Expresión Search
                //Expression<Func<Period, bool>> searchExpression;
                //if (!String.IsNullOrEmpty(search))
                //{
                //    searchExpression = x => x.History.Explanation.ToUpper().Contains(search);
                //}
                //else
                //{
                //    searchExpression = x => x.History.Explanation != null || x.History.Explanation == null;
                //}
                //#endregion Expresión Search

                #region Expresión Activo
                Expression<Func<Period, bool>> activeExpression;
                if (active.HasValue)
                {
                    activeExpression = x => x.Active == active.Value;
                }
                else
                {
                    activeExpression = x => x.Active == true || x.Active == false;
                }
                #endregion Expresión Activo

                var list = appDbContext.Periods
                                            .Where(activeExpression)
                                            .OrderByDescending(x => x.Year)
                                            .ThenByDescending(x => x.Month)
                                            .Skip(offset)
                                            .Take(limit)
                                            .ToList();

                return list;
            }
            catch (Exception e)
            {
                logger.Error($"{TAG} -- {e}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene la cantidad de registros encontrados
        /// </summary>
        /// <param name="search"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        public int Count(string search, bool? active)
        {
            try
            {
                //#region Expresión Search
                //Expression<Func<Period, bool>> searchExpression;
                //if (!String.IsNullOrEmpty(search))
                //{
                //    searchExpression = x => x.History.Explanation.ToUpper().Contains(search);
                //}
                //else
                //{
                //    searchExpression = x => x.History.Explanation != null || x.History.Explanation == null;
                //}
                //#endregion Expresión Search

                #region Expresión Activo
                Expression<Func<Period, bool>> activeExpression;
                if (active.HasValue)
                {
                    activeExpression = x => x.Active == active.Value;
                }
                else
                {
                    activeExpression = x => x.Active == true || x.Active == false;
                }
                #endregion Expresión Activo

                int count = appDbContext.Periods
                                            .Where(activeExpression)
                                            .OrderByDescending(x => x.Year)
                                            .ThenByDescending(x => x.Month)
                                            .Count();

                return count;
            }
            catch (Exception e)
            {
                logger.Error($"{TAG} -- {e}");
                throw;
            }
        }

        /// <summary>
        /// Registra un nuevo period
        /// </summary>
        /// <param name="period"></param>
        /// <param name="history"></param>
        /// <returns></returns>
        public bool Save(Period period, History history, HistoryPeriod historyPeriod)
        {
            bool success = false;

            try
            {
                #region TRANSACCION
                using (var dbContextTransaction = appDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        #region Period
                        appDbContext.Periods.Attach(period);
                        appDbContext.Entry(period).State = EntityState.Added;
                        success = (appDbContext.SaveChanges() > 0) ? true : false;
                        #endregion Period

                        #region History
                        if (success)
                        {
                            appDbContext.Histories.Attach(history);
                            appDbContext.Entry(history).State = EntityState.Added;
                            success = (appDbContext.SaveChanges() > 0) ? true : false;

                            if (success)
                            {
                                appDbContext.HistoryPeriods.Attach(historyPeriod);
                                appDbContext.Entry(historyPeriod).State = EntityState.Added;
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
        /// Actualiza un period
        /// </summary>
        /// <param name="period"></param>
        /// <param name="history"></param>
        /// <param name="historyPeriod"></param>
        /// <returns></returns>
        public bool Update(Period period, History history, HistoryPeriod historyPeriod)
        {
            bool success = false;

            try
            {
                #region TRANSACCION
                using (var dbContextTransaction = appDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        #region Period
                        appDbContext.Periods.Attach(period);
                        appDbContext.Entry(period).State = EntityState.Modified;
                        success = (appDbContext.SaveChanges() > 0) ? true : false;
                        #endregion Period

                        #region History
                        if (success)
                        {
                            appDbContext.Histories.Attach(history);
                            appDbContext.Entry(history).State = EntityState.Added;
                            success = (appDbContext.SaveChanges() > 0) ? true : false;

                            if (success)
                            {
                                appDbContext.HistoryPeriods.Attach(historyPeriod);
                                appDbContext.Entry(historyPeriod).State = EntityState.Added;
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
        /// Actualiza los datos de 2 periodos, con la finalidad de identificar un period actual y desactivar el anterior
        /// </summary>
        /// <param name="period"></param>
        /// <param name="history"></param>
        /// <param name="historyPeriod"></param>
        /// <param name="periodPrev"></param>
        /// <param name="historyPrev"></param>
        /// <param name="historyPeriodPrev"></param>
        /// <returns></returns>
        public bool Update(Period period, History history, HistoryPeriod historyPeriod, Period periodPrev, History historyPrev, HistoryPeriod historyPeriodPrev)
        {
            bool success = false;

            try
            {
                #region TRANSACCION
                using (var dbContextTransaction = appDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        #region Period
                        appDbContext.Periods.Attach(period);
                        appDbContext.Entry(period).State = EntityState.Modified;
                        success = (appDbContext.SaveChanges() > 0) ? true : false;
                        #endregion Period

                        #region History
                        if (success)
                        {
                            appDbContext.Histories.Attach(history);
                            appDbContext.Entry(history).State = EntityState.Added;
                            success = (appDbContext.SaveChanges() > 0) ? true : false;

                            if (success)
                            {
                                appDbContext.HistoryPeriods.Attach(historyPeriod);
                                appDbContext.Entry(historyPeriod).State = EntityState.Added;
                                success = (appDbContext.SaveChanges() > 0) ? true : false;
                            }
                        }
                        #endregion History

                        #region Period Anterior
                        if (success)
                        {
                            appDbContext.Periods.Attach(periodPrev);
                            appDbContext.Entry(periodPrev).State = EntityState.Modified;
                            success = (appDbContext.SaveChanges() > 0) ? true : false;
                        }
                        #endregion Period Anterior

                        #region History Period Anterior
                        if (success)
                        {
                            appDbContext.Histories.Attach(historyPrev);
                            appDbContext.Entry(historyPrev).State = EntityState.Added;
                            success = (appDbContext.SaveChanges() > 0) ? true : false;

                            if (success)
                            {
                                appDbContext.HistoryPeriods.Attach(historyPeriodPrev);
                                appDbContext.Entry(historyPeriodPrev).State = EntityState.Added;
                                success = (appDbContext.SaveChanges() > 0) ? true : false;
                            }
                        }
                        #endregion History Period Anterior
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
        #endregion Period

        #region History
        /// <summary>
        /// Obtiene listado de registros encontrados
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="period"></param>
        /// <param name="search"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        public List<HistoryPeriod> FindHistory(int limit, int offset, Period period, string search, bool? active)
        {
            try
            {
                #region Expresión Search
                Expression<Func<HistoryPeriod, bool>> searchExpression;
                if (!String.IsNullOrEmpty(search))
                {
                    searchExpression = x => x.History.Explanation.ToUpper().Contains(search);
                }
                else
                {
                    searchExpression = x => x.History.Explanation != null || x.History.Explanation == null;
                }
                #endregion Expresión Search

                var list = appDbContext.HistoryPeriods
                                            .Where(x => x.PeriodId == period.Id)
                                            .Where(x =>
                                                ((active.HasValue)
                                                    ? x.Active == active.Value
                                                    : (x.Active == true || x.Active == false)
                                                )
                                            )
                                            .Where(searchExpression)
                                            .OrderByDescending(x => x.CreatedDate)
                                            .Skip(offset)
                                            .Take(limit)
                                            .ToList();

                return list;
            }
            catch (Exception e)
            {
                logger.Error($"{TAG} -- {e}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene la cantidad de registros encontrados
        /// </summary>
        /// <param name="period"></param>
        /// <param name="search"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        public int CountHistory(Period period, string search, bool? active)
        {
            try
            {
                #region Expresión Search
                Expression<Func<HistoryPeriod, bool>> searchExpression;
                if (!String.IsNullOrEmpty(search))
                {
                    searchExpression = x => x.History.Explanation.ToUpper().Contains(search);
                }
                else
                {
                    searchExpression = x => x.History.Explanation != null || x.History.Explanation == null;
                }
                #endregion Expresión Search

                int count = appDbContext.HistoryPeriods
                                            .Where(x => x.PeriodId == period.Id)
                                            .Where(x =>
                                                ((active.HasValue)
                                                    ? x.Active == active.Value
                                                    : (x.Active == true || x.Active == false)
                                                )
                                            )
                                            .Where(searchExpression)
                                            .OrderByDescending(x => x.CreatedDate)
                                            .Count();

                return count;
            }
            catch (Exception e)
            {
                logger.Error($"{TAG} -- {e}");
                throw;
            }
        }
        #endregion History
    }
}
