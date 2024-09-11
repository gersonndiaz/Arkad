using Arkad.Server.Models;
using Arkad.Server.Models._Context;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Linq.Expressions;

namespace Arkad.Server.DAO.Impl
{
    public class ReparticionDao : IReparticionDao
    {
        private static string TAG = typeof(ReparticionDao).FullName;
        private Logger logger = LogManager.GetCurrentClassLogger();
        private AppDbContext appDbContext;

        #region Contructor
        /// <summary>
        /// Constructor por defecto, el cual tiene las configuraciones estándar para el acceso a datos
        /// </summary>
        /// <param name="appDbContext"></param>
        public ReparticionDao()
        {
            this.appDbContext = (appDbContext != null) ? appDbContext : new AppDbContext();
        }

        /// <summary>
        /// Constructor que recibe el contexto
        /// </summary>
        /// <param name="appDbContext"></param>
        public ReparticionDao(AppDbContext appDbContext)
        {
            this.appDbContext = (appDbContext != null) ? appDbContext : new AppDbContext();
        }
        #endregion Constructor

        #region Reparticion
        /// <summary>
        /// Obtiene una repartición por su ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Reparticion GetById(string id)
        {
            try
            {
                var reparticion = appDbContext.Reparticiones
                                    .Where(x => x.Id == id)
                                    .FirstOrDefault();
                return reparticion;
            }
            catch (Exception ex)
            {
                logger.Error($"[{TAG}] - {ex}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene una repartición por su Código
        /// </summary>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public Reparticion GetByCodigo(string codigo)
        {
            try
            {
                var reparticion = appDbContext.Reparticiones
                                    .Where(x => x.Codigo == codigo)
                                    .FirstOrDefault();
                return reparticion;
            }
            catch (Exception ex)
            {
                logger.Error($"[{TAG}] - {ex}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene na repartición por su Nombre
        /// </summary>
        /// <param name="nombre"></param>
        /// <returns></returns>
        public Reparticion GetByNombre(string nombre)
        {
            try
            {
                var reparticion = appDbContext.Reparticiones
                                    .Where(x => x.Nombre == nombre)
                                    .FirstOrDefault();
                return reparticion;
            }
            catch (Exception ex)
            {
                logger.Error($"[{TAG}] - {ex}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene listado de reparticiones
        /// </summary>
        /// <param name="activo"></param>
        /// <returns></returns>
        public List<Reparticion> GetAll(bool? activo)
        {
            try
            {
                var list = appDbContext.Reparticiones
                                    .Where(x => x.BActivo == activo)
                                    .OrderBy(x => x.Nombre)
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
        /// Se registra repartición
        /// </summary>
        /// <param name="reparticion"></param>
        /// <param name="historial"></param>
        /// <returns></returns>
        public bool Save(Reparticion reparticion, HistorialReparticion historial)
        {
            bool success = false;

            try
            {
                #region TRANSACCION
                using (var dbContextTransaction = appDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        #region Reparticion
                        appDbContext.Reparticiones.Attach(reparticion);
                        appDbContext.Entry(reparticion).State = EntityState.Added;
                        success = (appDbContext.SaveChanges() > 0) ? true : false;
                        #endregion Reparticion

                        #region Historial
                        if (success)
                        {
                            appDbContext.HistorialReparticion.Attach(historial);
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
        /// Se actualiza repartición
        /// </summary>
        /// <param name="reparticion"></param>
        /// <param name="historial"></param>
        /// <returns></returns>
        public bool Update(Reparticion reparticion, HistorialReparticion historial)
        {
            bool success = false;

            try
            {
                #region TRANSACCION
                using (var dbContextTransaction = appDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        #region Reparticion
                        appDbContext.Reparticiones.Attach(reparticion);
                        appDbContext.Entry(reparticion).State = EntityState.Modified;
                        success = (appDbContext.SaveChanges() > 0) ? true : false;
                        #endregion Reparticion

                        #region Historial
                        if (success)
                        {
                            appDbContext.HistorialReparticion.Attach(historial);
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
        #endregion Reparticion

        #region Historial
        /// <summary>
        /// Obtiene listado de registros encontrados
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="reparticion"></param>
        /// <param name="search"></param>
        /// <param name="activo"></param>
        /// <returns></returns>
        public List<HistorialReparticion> FindHistorial(int limit, int offset, Reparticion reparticion, string search, bool? activo)
        {
            try
            {
                #region Expresión Search
                Expression<Func<HistorialReparticion, bool>> searchExpression;
                if (!String.IsNullOrEmpty(search))
                {
                    searchExpression = x => x.Descripcion.Contains(search);
                }
                else
                {
                    searchExpression = x => x.Descripcion != null || x.Descripcion == null;
                }
                #endregion Expresión Search

                var list = appDbContext.HistorialReparticion
                                            .Where(x => x.ReparticionId == reparticion.Id)
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
        /// <param name="reparticion"></param>
        /// <param name="search"></param>
        /// <param name="activo"></param>
        /// <returns></returns>
        public int CountHistorial(Reparticion reparticion, string search, bool? activo)
        {
            try
            {
                #region Expresión Search
                Expression<Func<HistorialReparticion, bool>> searchExpression;
                if (!String.IsNullOrEmpty(search))
                {
                    searchExpression = x => x.Descripcion.Contains(search);
                }
                else
                {
                    searchExpression = x => x.Descripcion != null || x.Descripcion == null;
                }
                #endregion Expresión Search

                int count = appDbContext.HistorialReparticion
                                            .Where(x => x.ReparticionId == reparticion.Id)
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
