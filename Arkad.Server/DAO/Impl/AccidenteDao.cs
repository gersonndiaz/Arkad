using Arkad.Server.Models;
using Arkad.Server.Models._Context;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Linq.Expressions;

namespace Arkad.Server.DAO.Impl
{
    public class AccidenteDao : IAccidenteDao
    {
        private static string TAG = typeof(AccidenteDao).FullName;
        private Logger logger = LogManager.GetCurrentClassLogger();
        private AppDbContext appDbContext;

        #region Contructor
        /// <summary>
        /// Constructor por defecto, el cual tiene las configuraciones estándar para el acceso a datos
        /// </summary>
        /// <param name="appDbContext"></param>
        public AccidenteDao()
        {
            this.appDbContext = (appDbContext != null) ? appDbContext : new AppDbContext();
        }

        /// <summary>
        /// Constructor que recibe el contexto
        /// </summary>
        /// <param name="appDbContext"></param>
        public AccidenteDao(AppDbContext appDbContext)
        {
            this.appDbContext = (appDbContext != null) ? appDbContext : new AppDbContext();
        }
        #endregion Constructor

        #region Accidente
        /// <summary>
        /// Obtiene Accidente por su ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Accidente GetById(string id)
        {
            try
            {
                var accidente = appDbContext.Accidentes
                                    .Where(x => x.Id == id)
                                    .FirstOrDefault();
                return accidente;
            }
            catch (Exception ex)
            {
                logger.Error($"[{TAG}] - {ex}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un Accidente
        /// </summary>
        /// <param name="periodo"></param>
        /// <param name="reparticion"></param>
        /// <param name="item"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public Accidente GetCurrent(Periodo periodo, Reparticion reparticion, Item item, string tipo)
        {
            try
            {
                var accidente = appDbContext.Accidentes
                                    .Where(x => x.PeriodoId == periodo.Id)
                                    .Where(x => x.ReparticionId == reparticion.Id)
                                    .Where(x => x.ItemId == item.Id)
                                    .Where(x=> x.Tipo == tipo)
                                    .FirstOrDefault();
                return accidente;
            }
            catch (Exception ex)
            {
                logger.Error($"[{TAG}] - {ex}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene listado de accidentes encontrados
        /// </summary>
        /// <param name="periodo"></param>
        /// <param name="reparticion"></param>
        /// <param name="activo"></param>
        /// <returns></returns>
        public List<Accidente> GetAll(Periodo periodo, Reparticion reparticion, bool? activo)
        {
            try
            {
                #region Expresión Activo
                Expression<Func<Accidente, bool>> activeExpression;
                if (activo.HasValue)
                {
                    activeExpression = x => x.BActivo == activo.Value;
                }
                else
                {
                    activeExpression = x => x.BActivo == true || x.BActivo == false;
                }
                #endregion Expresión Activo

                var list = appDbContext.Accidentes
                                            .Where(x => x.ReparticionId == reparticion.Id)
                                            .Where(x => x.PeriodoId == periodo.Id)
                                            .Where(activeExpression)
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
        /// Registra la información
        /// </summary>
        /// <param name="accidentes"></param>
        /// <param name="accidentesMod"></param>
        /// <returns></returns>
        public bool Save(List<Accidente> accidentes, List<Accidente> accidentesMod, HistorialPeriodoAccidentes historial)
        {
            bool success = false;

            try
            {
                #region TRANSACCION
                using (var dbContextTransaction = appDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        #region Nuevos
                        if (accidentes != null && accidentes.Count > 0)
                        {
                            int countErr = 0;
                            foreach (var accidente in accidentes)
                            {
                                appDbContext.Accidentes.Attach(accidente);
                                appDbContext.Entry(accidente).State = EntityState.Added;
                                countErr += (appDbContext.SaveChanges() > 0) ? 0 : 1;
                            }

                            success = (countErr > 0) ? false : true;
                        }
                        else
                        {
                            success = true;
                        }
                        #endregion Nuevso

                        #region Modificado
                        if (success)
                        {
                            if (accidentesMod != null && accidentesMod.Count > 0)
                            {
                                int countErr = 0;
                                foreach (var accidente in accidentesMod)
                                {
                                    appDbContext.Accidentes.Attach(accidente);
                                    appDbContext.Entry(accidente).State = EntityState.Modified;
                                    countErr += (appDbContext.SaveChanges() > 0) ? 0 : 1;
                                }

                                success = (countErr > 0) ? false : true;
                            }
                        }
                        #endregion Modificado

                        #region Historial
                        if (success)
                        {
                            appDbContext.HistorialPeriodoAccidentes.Attach(historial);
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
        /// Registra la información
        /// </summary>
        /// <param name="accidentes"></param>
        /// <param name="accidentesMod"></param>
        /// <param name="indicadorPeriodo"></param>
        /// <param name="historial"></param>
        /// <returns></returns>
        public bool Save(List<Accidente> accidentes, List<Accidente> accidentesMod, IndicadorPeriodo indicadorPeriodo, HistorialPeriodoAccidentes historial)
        {
            bool success = false;

            try
            {
                #region TRANSACCION
                using (var dbContextTransaction = appDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        #region Nuevos
                        if (accidentes != null && accidentes.Count > 0)
                        {
                            int countErr = 0;
                            foreach (var accidente in accidentes)
                            {
                                appDbContext.Accidentes.Attach(accidente);
                                appDbContext.Entry(accidente).State = EntityState.Added;
                                countErr += (appDbContext.SaveChanges() > 0) ? 0 : 1;
                            }

                            success = (countErr > 0) ? false : true;
                        }
                        else
                        {
                            success = true;
                        }
                        #endregion Nuevso

                        #region Modificado
                        if (success)
                        {
                            if (accidentesMod != null && accidentesMod.Count > 0)
                            {
                                int countErr = 0;
                                foreach (var accidente in accidentesMod)
                                {
                                    appDbContext.Accidentes.Attach(accidente);
                                    appDbContext.Entry(accidente).State = EntityState.Modified;
                                    countErr += (appDbContext.SaveChanges() > 0) ? 0 : 1;
                                }

                                success = (countErr > 0) ? false : true;
                            }
                        }
                        #endregion Modificado

                        #region Indicador
                        if (success)
                        {
                            appDbContext.IndicadorPeriodos.Attach(indicadorPeriodo);
                            appDbContext.Entry(indicadorPeriodo).State = EntityState.Added;
                            success = (appDbContext.SaveChanges() > 0) ? true : false;
                        }
                        #endregion Indicador

                        #region Historial
                        if (success)
                        {
                            appDbContext.HistorialPeriodoAccidentes.Attach(historial);
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
        #endregion Accidente

        #region Indicador
        /// <summary>
        /// Obtiene el indicador por su ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IndicadorPeriodo GetIndicadorById(string id)
        {
            try
            {
                var indicador = appDbContext.IndicadorPeriodos
                                    .Where(x => x.Id == id)
                                    .FirstOrDefault();
                return indicador;
            }
            catch (Exception ex)
            {
                logger.Error($"[{TAG}] - {ex}");
                throw;
            }
        }
        /// <summary>
        /// Obtiene el último indicador creado y activo
        /// </summary>
        /// <param name="periodo"></param>
        /// <param name="reparticion"></param>
        /// <returns></returns>
        public IndicadorPeriodo GetIndicador(Periodo periodo, Reparticion reparticion)
        {
            try
            {
                var indicador = appDbContext.IndicadorPeriodos
                                    .Where(x => x.PeriodoId == periodo.Id)
                                    .Where(x => x.ReparticionId == reparticion.Id)
                                    .Where(x => x.BActivo)
                                    .OrderByDescending(x => x.FCreado)
                                    .FirstOrDefault();
                return indicador;
            }
            catch (Exception ex)
            {
                logger.Error($"[{TAG}] - {ex}");
                throw;
            }
        }

        /// <summary>
        /// Actualiza información del indicador
        /// </summary>
        /// <param name="indicadorPeriodo"></param>
        /// <param name="historial"></param>
        /// <returns></returns>
        public bool UpdateIndicador(IndicadorPeriodo indicadorPeriodo, HistorialPeriodoAccidentes historial)
        {
            bool success = false;

            try
            {
                #region TRANSACCION
                using (var dbContextTransaction = appDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        #region Indicador
                        appDbContext.IndicadorPeriodos.Attach(indicadorPeriodo);
                        appDbContext.Entry(indicadorPeriodo).State = EntityState.Modified;
                        success = (appDbContext.SaveChanges() > 0) ? true : false;
                        #endregion Indicador

                        #region Historial
                        if (success)
                        {
                            appDbContext.HistorialPeriodoAccidentes.Attach(historial);
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
        #endregion Indicador
    }
}
