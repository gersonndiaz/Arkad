using Arkad.Server.Models;
using Arkad.Server.Models._Context;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Linq.Expressions;

namespace Arkad.Server.DAO.Impl
{
    public class ItemDao : IItemDao
    {
        private static string TAG = typeof(ItemDao).FullName;
        private Logger logger = LogManager.GetCurrentClassLogger();
        private AppDbContext appDbContext;

        #region Contructor
        /// <summary>
        /// Constructor por defecto, el cual tiene las configuraciones estándar para el acceso a datos
        /// </summary>
        /// <param name="appDbContext"></param>
        public ItemDao()
        {
            this.appDbContext = (appDbContext != null) ? appDbContext : new AppDbContext();
        }

        /// <summary>
        /// Constructor que recibe el contexto
        /// </summary>
        /// <param name="appDbContext"></param>
        public ItemDao(AppDbContext appDbContext)
        {
            this.appDbContext = (appDbContext != null) ? appDbContext : new AppDbContext();
        }
        #endregion Constructor

        #region Item
        /// <summary>
        /// Obtiene un item por su ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Item GetById(string id)
        {
            try
            {
                var item = appDbContext.Items
                                    .Where(x => x.Id == id)
                                    .FirstOrDefault();
                return item;
            }
            catch (Exception ex)
            {
                logger.Error($"[{TAG}] - {ex}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un item por su Nombre
        /// </summary>
        /// <param name="nombre"></param>
        /// <returns></returns>
        public Item GetByNombre(string nombre)
        {
            try
            {
                var item = appDbContext.Items
                                    .Where(x => x.Nombre == nombre)
                                    .FirstOrDefault();
                return item;
            }
            catch (Exception ex)
            {
                logger.Error($"[{TAG}] - {ex}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene listado de items
        /// </summary>
        /// <param name="activo"></param>
        /// <returns></returns>
        public List<Item> GetAll(bool? activo)
        {
            try
            {
                #region Expresión Activo
                Expression<Func<Item, bool>> activeExpression;
                if (activo.HasValue)
                {
                    activeExpression = x => x.BActivo == activo.Value;
                }
                else
                {
                    activeExpression = x => x.BActivo == true || x.BActivo == false;
                }
                #endregion Expresión Activo

                var list = appDbContext.Items
                                    .Where(activeExpression)
                                    .OrderBy(x => x.Posicion)
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
        /// Registra un nuevo ítem
        /// </summary>
        /// <param name="item"></param>
        /// <param name="historial"></param>
        /// <returns></returns>
        public bool Save(Item item, HistorialItem historial)
        {
            bool success = false;

            try
            {
                #region TRANSACCION
                using (var dbContextTransaction = appDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        #region Ítem
                        appDbContext.Items.Attach(item);
                        appDbContext.Entry(item).State = EntityState.Added;
                        success = (appDbContext.SaveChanges() > 0) ? true : false;
                        #endregion Ítem

                        #region Historial
                        if (success)
                        {
                            appDbContext.HistorialItems.Attach(historial);
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
        /// Actualiza un ítem
        /// </summary>
        /// <param name="item"></param>
        /// <param name="historial"></param>
        /// <returns></returns>
        public bool Update(Item item, HistorialItem historial)
        {
            bool success = false;

            try
            {
                #region TRANSACCION
                using (var dbContextTransaction = appDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        #region Item
                        appDbContext.Items.Attach(item);
                        appDbContext.Entry(item).State = EntityState.Modified;
                        success = (appDbContext.SaveChanges() > 0) ? true : false;
                        #endregion Item

                        #region Historial
                        if (success)
                        {
                            appDbContext.HistorialItems.Attach(historial);
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
        /// Actualiza listado de ítems
        /// </summary>
        /// <param name="items"></param>
        /// <param name="historial"></param>
        /// <returns></returns>
        public bool Update(List<Item> items, List<HistorialItem> historial)
        {
            bool success = false;

            try
            {
                #region TRANSACCION
                using (var dbContextTransaction = appDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        #region Items
                        if (items != null && items.Count > 0)
                        {
                            int countErr = 0;
                            foreach (var item in items)
                            {
                                appDbContext.Items.Attach(item);
                                appDbContext.Entry(item).State = EntityState.Modified;
                                countErr += (appDbContext.SaveChanges() > 0) ? 0 : 1;
                            }

                            success = (countErr > 0) ? false : true;
                        }
                        #endregion Items

                        #region Historial
                        if (success)
                        {
                            if (historial != null && historial.Count > 0)
                            {
                                int countErr = 0;
                                foreach (var h in historial)
                                {
                                    appDbContext.HistorialItems.Attach(h);
                                    appDbContext.Entry(h).State = EntityState.Added;
                                    countErr += (appDbContext.SaveChanges() > 0) ? 0 : 1;
                                }

                                success = (countErr > 0) ? false : true;
                            }
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
        #endregion Item

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
        public List<HistorialItem> FindHistorial(int limit, int offset, Item item, string search, bool? activo)
        {
            try
            {
                #region Expresión Search
                Expression<Func<HistorialItem, bool>> searchExpression;
                if (!String.IsNullOrEmpty(search))
                {
                    searchExpression = x => x.Descripcion.ToUpper().Contains(search);
                }
                else
                {
                    searchExpression = x => x.Descripcion != null || x.Descripcion == null;
                }
                #endregion Expresión Search

                var list = appDbContext.HistorialItems
                                            .Where(x => x.ItemId == item.Id)
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
        /// <param name="item"></param>
        /// <param name="search"></param>
        /// <param name="activo"></param>
        /// <returns></returns>
        public int CountHistorial(Item item, string search, bool? activo)
        {
            try
            {
                #region Expresión Search
                Expression<Func<HistorialItem, bool>> searchExpression;
                if (!String.IsNullOrEmpty(search))
                {
                    searchExpression = x => x.Descripcion.ToUpper().Contains(search);
                }
                else
                {
                    searchExpression = x => x.Descripcion != null || x.Descripcion == null;
                }
                #endregion Expresión Search

                int count = appDbContext.HistorialItems
                                            .Where(x => x.ItemId == item.Id)
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
