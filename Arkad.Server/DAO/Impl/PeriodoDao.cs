using Arkad.Server.Models;
using Arkad.Server.Models._Context;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Linq;
using System.Linq.Expressions;

namespace Arkad.Server.DAO.Impl
{
    public class PeriodoDao : IPeriodoDao
    {
        private static string TAG = typeof(PeriodoDao).FullName;
        private Logger logger = LogManager.GetCurrentClassLogger();
        private AppDbContext appDbContext;

        #region Contructor
        /// <summary>
        /// Constructor por defecto, el cual tiene las configuraciones estándar para el acceso a datos
        /// </summary>
        /// <param name="appDbContext"></param>
        public PeriodoDao()
        {
            this.appDbContext = (appDbContext != null) ? appDbContext : new AppDbContext();
        }

        /// <summary>
        /// Constructor que recibe el contexto
        /// </summary>
        /// <param name="appDbContext"></param>
        public PeriodoDao(AppDbContext appDbContext)
        {
            this.appDbContext = (appDbContext != null) ? appDbContext : new AppDbContext();
        }
        #endregion Constructor

        #region Periodo
        /// <summary>
        /// Obtiene un periodo por su ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Periodo GetById(string id)
        {
            try
            {
                var periodo = appDbContext.Periodos
                                    .Where(x => x.Id == id)
                                    .FirstOrDefault();
                return periodo;
            }
            catch (Exception ex)
            {
                logger.Error($"[{TAG}] - {ex}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene el periodo activo actual
        /// </summary>
        /// <returns></returns>
        public Periodo GetCurrent()
        {
            try
            {
                var periodo = appDbContext.Periodos
                                    .Where(x => x.BActual == true)
                                    .FirstOrDefault();
                return periodo;
            }
            catch (Exception ex)
            {
                logger.Error($"[{TAG}] - {ex}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un periodo especifico 
        /// </summary>
        /// <param name="anio"></param>
        /// <param name="mes"></param>
        /// <returns></returns>
        public Periodo GetByAnioMes(int anio, int mes)
        {
            try
            {
                var periodo = appDbContext.Periodos
                                    .Where(x => x.Anio == anio)
                                    .Where(x => x.Mes == mes)
                                    .FirstOrDefault();
                return periodo;
            }
            catch (Exception ex)
            {
                logger.Error($"[{TAG}] - {ex}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene listado de Periodos encontrados
        /// </summary>
        /// <param name="activo"></param>
        /// <returns></returns>
        public List<Periodo> GetAll(bool? activo)
        {
            try
            {
                #region Expresión Activo
                Expression<Func<Periodo, bool>> activeExpression;
                if (activo.HasValue)
                {
                    activeExpression = x => x.BActivo == activo.Value;
                }
                else
                {
                    activeExpression = x => x.BActivo == true || x.BActivo == false;
                }
                #endregion Expresión Activo

                var list = appDbContext.Periodos
                                    .Where(activeExpression)
                                    .OrderByDescending(x => x.Anio)
                                    .ThenByDescending(x => x.Mes)
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
        /// Obtiene listado de Periodos encontrados
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="search"></param>
        /// <param name="activo"></param>
        /// <returns></returns>
        public List<Periodo> GetAll(int limit, int offset, string search, bool? activo)
        {
            try
            {
                //#region Expresión Search
                //Expression<Func<Periodo, bool>> searchExpression;
                //if (!String.IsNullOrEmpty(search))
                //{
                //    searchExpression = x => x.Descripcion.ToUpper().Contains(search);
                //}
                //else
                //{
                //    searchExpression = x => x.Descripcion != null || x.Descripcion == null;
                //}
                //#endregion Expresión Search

                #region Expresión Activo
                Expression<Func<Periodo, bool>> activeExpression;
                if (activo.HasValue)
                {
                    activeExpression = x => x.BActivo == activo.Value;
                }
                else
                {
                    activeExpression = x => x.BActivo == true || x.BActivo == false;
                }
                #endregion Expresión Activo

                var list = appDbContext.Periodos
                                            .Where(activeExpression)
                                            .OrderByDescending(x => x.Anio)
                                            .ThenByDescending(x => x.Mes)
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
        /// <param name="activo"></param>
        /// <returns></returns>
        public int Count(string search, bool? activo)
        {
            try
            {
                //#region Expresión Search
                //Expression<Func<Periodo, bool>> searchExpression;
                //if (!String.IsNullOrEmpty(search))
                //{
                //    searchExpression = x => x.Descripcion.ToUpper().Contains(search);
                //}
                //else
                //{
                //    searchExpression = x => x.Descripcion != null || x.Descripcion == null;
                //}
                //#endregion Expresión Search

                #region Expresión Activo
                Expression<Func<Periodo, bool>> activeExpression;
                if (activo.HasValue)
                {
                    activeExpression = x => x.BActivo == activo.Value;
                }
                else
                {
                    activeExpression = x => x.BActivo == true || x.BActivo == false;
                }
                #endregion Expresión Activo

                int count = appDbContext.Periodos
                                            .Where(activeExpression)
                                            .OrderByDescending(x => x.Anio)
                                            .ThenByDescending(x => x.Mes)
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
        /// Registra un nuevo periodo
        /// </summary>
        /// <param name="periodo"></param>
        /// <param name="historial"></param>
        /// <returns></returns>
        public bool Save(Periodo periodo, HistorialPeriodo historial)
        {
            bool success = false;

            try
            {
                #region TRANSACCION
                using (var dbContextTransaction = appDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        #region Periodo
                        appDbContext.Periodos.Attach(periodo);
                        appDbContext.Entry(periodo).State = EntityState.Added;
                        success = (appDbContext.SaveChanges() > 0) ? true : false;
                        #endregion Periodo

                        #region Historial
                        if (success)
                        {
                            appDbContext.HistorialPeriodos.Attach(historial);
                            appDbContext.Entry(historial).State = EntityState.Added;
                            success = (appDbContext.SaveChanges() > 0) ? true : false;
                        }
                        #endregion Historial
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
        /// Actualiza un periodo
        /// </summary>
        /// <param name="periodo"></param>
        /// <param name="historial"></param>
        /// <returns></returns>
        public bool Update(Periodo periodo, HistorialPeriodo historial)
        {
            bool success = false;

            try
            {
                #region TRANSACCION
                using (var dbContextTransaction = appDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        #region Periodo
                        appDbContext.Periodos.Attach(periodo);
                        appDbContext.Entry(periodo).State = EntityState.Modified;
                        success = (appDbContext.SaveChanges() > 0) ? true : false;
                        #endregion Periodo

                        #region Historial
                        if (success)
                        {
                            appDbContext.HistorialPeriodos.Attach(historial);
                            appDbContext.Entry(historial).State = EntityState.Added;
                            success = (appDbContext.SaveChanges() > 0) ? true : false;
                        }
                        #endregion Historial
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
        /// Actualiza los datos de 2 periodos, con la finalidad de identificar un periodo actual y desactivar el anterior
        /// </summary>
        /// <param name="periodo"></param>
        /// <param name="historial"></param>
        /// <param name="periodoPrev"></param>
        /// <param name="historialPrev"></param>
        /// <returns></returns>
        public bool Update(Periodo periodo, HistorialPeriodo historial, Periodo periodoPrev, HistorialPeriodo historialPrev)
        {
            bool success = false;

            try
            {
                #region TRANSACCION
                using (var dbContextTransaction = appDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        #region Periodo
                        appDbContext.Periodos.Attach(periodo);
                        appDbContext.Entry(periodo).State = EntityState.Modified;
                        success = (appDbContext.SaveChanges() > 0) ? true : false;
                        #endregion Periodo

                        #region Historial
                        if (success)
                        {
                            appDbContext.HistorialPeriodos.Attach(historial);
                            appDbContext.Entry(historial).State = EntityState.Added;
                            success = (appDbContext.SaveChanges() > 0) ? true : false;
                        }
                        #endregion Historial

                        #region Periodo Anterior
                        if (success)
                        {
                            appDbContext.Periodos.Attach(periodoPrev);
                            appDbContext.Entry(periodoPrev).State = EntityState.Modified;
                            success = (appDbContext.SaveChanges() > 0) ? true : false;
                        }
                        #endregion Periodo Anterior

                        #region Historial Periodo Anterior
                        if (success)
                        {
                            appDbContext.HistorialPeriodos.Attach(historialPrev);
                            appDbContext.Entry(historialPrev).State = EntityState.Added;
                            success = (appDbContext.SaveChanges() > 0) ? true : false;
                        }
                        #endregion Historial Periodo Anterior
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
        #endregion Periodo

        #region Historial
        /// <summary>
        /// Obtiene listado de registros encontrados
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="item"></param>
        /// <param name="search"></param>
        /// <param name="activo"></param>
        /// <returns></returns>
        public List<HistorialPeriodo> FindHistorial(int limit, int offset, Periodo periodo, string search, bool? activo)
        {
            try
            {
                #region Expresión Search
                Expression<Func<HistorialPeriodo, bool>> searchExpression;
                if (!String.IsNullOrEmpty(search))
                {
                    searchExpression = x => x.Descripcion.ToUpper().Contains(search);
                }
                else
                {
                    searchExpression = x => x.Descripcion != null || x.Descripcion == null;
                }
                #endregion Expresión Search

                var list = appDbContext.HistorialPeriodos
                                            .Where(x => x.PeriodoId == periodo.Id)
                                            .Where(x =>
                                                ((activo.HasValue)
                                                    ? x.BActivo == activo.Value
                                                    : (x.BActivo == true || x.BActivo == false)
                                                )
                                            )
                                            .Where(searchExpression)
                                            .OrderByDescending(x => x.FCreado)
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
        /// <param name="periodo"></param>
        /// <param name="search"></param>
        /// <param name="activo"></param>
        /// <returns></returns>
        public int CountHistorial(Periodo periodo, string search, bool? activo)
        {
            try
            {
                #region Expresión Search
                Expression<Func<HistorialPeriodo, bool>> searchExpression;
                if (!String.IsNullOrEmpty(search))
                {
                    searchExpression = x => x.Descripcion.ToUpper().Contains(search);
                }
                else
                {
                    searchExpression = x => x.Descripcion != null || x.Descripcion == null;
                }
                #endregion Expresión Search

                int count = appDbContext.HistorialPeriodos
                                            .Where(x => x.PeriodoId == periodo.Id)
                                            .Where(x =>
                                                ((activo.HasValue)
                                                    ? x.BActivo == activo.Value
                                                    : (x.BActivo == true || x.BActivo == false)
                                                )
                                            )
                                            .Where(searchExpression)
                                            .OrderByDescending(x => x.FCreado)
                                            .Count();

                return count;
            }
            catch (Exception e)
            {
                logger.Error($"{TAG} -- {e}");
                throw;
            }
        }
        #endregion Historial
    }
}
