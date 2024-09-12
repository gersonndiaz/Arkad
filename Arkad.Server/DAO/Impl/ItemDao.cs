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
            appDbContext = appDbContext != null ? appDbContext : new AppDbContext();
        }

        /// <summary>
        /// Constructor que recibe el contexto
        /// </summary>
        /// <param name="appDbContext"></param>
        public ItemDao(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext != null ? appDbContext : new AppDbContext();
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
        /// Obtiene un item por su Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Item GetByName(string name)
        {
            try
            {
                var item = appDbContext.Items
                                    .Where(x => x.Name == name)
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
                    activeExpression = x => x.Active == activo.Value;
                }
                else
                {
                    activeExpression = x => x.Active == true || x.Active == false;
                }
                #endregion Expresión Activo

                var list = appDbContext.Items
                                    .Where(activeExpression)
                                    .OrderBy(x => x.Position)
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
        public bool Save(Item item, History history, HistoryItem historyItem)
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
                        success = appDbContext.SaveChanges() > 0 ? true : false;
                        #endregion Ítem

                        #region History
                        if (success)
                        {
                            appDbContext.Histories.Attach(history);
                            appDbContext.Entry(history).State = EntityState.Added;
                            success = (appDbContext.SaveChanges() > 0) ? true : false;

                            if (success)
                            {
                                appDbContext.HistoryItems.Attach(historyItem);
                                appDbContext.Entry(historyItem).State = EntityState.Added;
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
        /// Actualiza un ítem
        /// </summary>
        /// <param name="item"></param>
        /// <param name="historial"></param>
        /// <returns></returns>
        public bool Update(Item item, History history, HistoryItem historyItem)
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
                        success = appDbContext.SaveChanges() > 0 ? true : false;
                        #endregion Item

                        #region History
                        if (success)
                        {
                            appDbContext.Histories.Attach(history);
                            appDbContext.Entry(history).State = EntityState.Added;
                            success = (appDbContext.SaveChanges() > 0) ? true : false;

                            if (success)
                            {
                                appDbContext.HistoryItems.Attach(historyItem);
                                appDbContext.Entry(historyItem).State = EntityState.Added;
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
        /// Actualiza listado de ítems
        /// </summary>
        /// <param name="items"></param>
        /// <param name="historial"></param>
        /// <returns></returns>
        public bool Update(List<Item> items, List<History> histories, List<HistoryItem> historiesItem)
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
                                countErr += appDbContext.SaveChanges() > 0 ? 0 : 1;
                            }

                            success = countErr > 0 ? false : true;
                        }
                        #endregion Items

                        #region History
                        if (success)
                        {
                            if (histories != null && histories.Count > 0)
                            {
                                int countErr = 0;
                                foreach (var history in histories)
                                {
                                    appDbContext.Histories.Attach(history);
                                    appDbContext.Entry(history).State = EntityState.Added;
                                    success = (appDbContext.SaveChanges() > 0) ? true : false;
                                }

                                success = countErr > 0 ? false : true;
                            }

                            if (success)
                            {
                                if (historiesItem != null && historiesItem.Count > 0)
                                {
                                    int countErr = 0;
                                    foreach (var historyItem in historiesItem)
                                    {
                                        appDbContext.HistoryItems.Attach(historyItem);
                                        appDbContext.Entry(historyItem).State = EntityState.Added;
                                        success = (appDbContext.SaveChanges() > 0) ? true : false;
                                    }

                                    success = countErr > 0 ? false : true;
                                }
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
        public List<HistoryItem> FindHistories(int limit, int offset, Item item, string search, bool? activo)
        {
            try
            {
                #region Expresión Search
                Expression<Func<HistoryItem, bool>> searchExpression;
                if (!string.IsNullOrEmpty(search))
                {
                    searchExpression = x => x.History.Description.ToUpper().Contains(search);
                }
                else
                {
                    searchExpression = x => x.History.Description != null || x.History.Description == null;
                }
                #endregion Expresión Search

                var list = appDbContext.HistoryItems
                                            .Where(x => x.ItemId == item.Id)
                                            .Where(x =>
                                                activo.HasValue
                                                    ? x.Active == activo.Value
                                                    : x.Active == true || x.Active == false

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
        /// <param name="item"></param>
        /// <param name="search"></param>
        /// <param name="activo"></param>
        /// <returns></returns>
        public int CountHistories(Item item, string search, bool? activo)
        {
            try
            {
                #region Expresión Search
                Expression<Func<HistoryItem, bool>> searchExpression;
                if (!string.IsNullOrEmpty(search))
                {
                    searchExpression = x => x.History.Description.ToUpper().Contains(search);
                }
                else
                {
                    searchExpression = x => x.History.Description != null || x.History.Description == null;
                }
                #endregion Expresión Search

                int count = appDbContext.HistoryItems
                                            .Where(x => x.ItemId == item.Id)
                                            .Where(x =>
                                                activo.HasValue
                                                    ? x.Active == activo.Value
                                                    : x.Active == true || x.Active == false

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
        #endregion Historial
    }
}
