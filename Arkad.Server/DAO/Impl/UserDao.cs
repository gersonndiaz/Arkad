using Arkad.Server.Models;
using Arkad.Server.Models._Context;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Linq.Expressions;

namespace Arkad.Server.DAO.Impl
{
    public class UserDao : IUserDao
    {
        private static string TAG = typeof(UserDao).FullName;
        private Logger logger = LogManager.GetCurrentClassLogger();
        private AppDbContext appDbContext;

        #region Contructor
        /// <summary>
        /// Constructor por defecto, el cual tiene las configuraciones estándar para el acceso a datos
        /// </summary>
        /// <param name="appDbContext"></param>
        public UserDao()
        {
            this.appDbContext = (appDbContext != null) ? appDbContext : new AppDbContext();
        }

        /// <summary>
        /// Constructor que recibe el contexto
        /// </summary>
        /// <param name="appDbContext"></param>
        public UserDao(AppDbContext appDbContext)
        {
            this.appDbContext = (appDbContext != null) ? appDbContext : new AppDbContext();
        }
        #endregion Constructor

        #region User
        /// <summary>
        /// Obtiene un usuario por su ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public User GetById(string id)
        {
            try
            {
                var usuario = appDbContext.Users
                                    .Where(x => x.Id == id)
                                    .FirstOrDefault();
                return usuario;
            }
            catch(Exception ex)
            {
                logger.Error($"[{TAG}] - {ex}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene usuario por su Correo
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public User GetByEmail(string email)
        {
            try
            {
                var usuario = appDbContext.Users
                                    .Where(x => x.Email.ToUpper() == email.ToUpper())
                                    .FirstOrDefault();
                return usuario;
            }
            catch (Exception ex)
            {
                logger.Error($"[{TAG}] - {ex}");
                throw;
            }
        }

        /// <summary>
        /// Actualiza datos del usuario
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool Update(User user)
        {
            bool success = false;

            try
            {
                #region TRANSACCION
                using (var dbContextTransaction = appDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        appDbContext.Users.Attach(user);
                        appDbContext.Entry(user).State = EntityState.Modified;
                        success = (appDbContext.SaveChanges() > 0) ? true : false;
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
                    #endregion COMMIT OR ROLLBACK TRANSACTION
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
        /// Actualiza información de usuario
        /// </summary>
        /// <param name="user"></param>
        /// <param name="history"></param>
        /// <param name="historyUser"></param>
        /// <returns></returns>
        public bool Update(User user, History history, HistoryUser historyUser)
        {
            bool success = false;

            try
            {
                #region TRANSACCION
                using (var dbContextTransaction = appDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        appDbContext.Users.Attach(user);
                        appDbContext.Entry(user).State = EntityState.Modified;
                        success = (appDbContext.SaveChanges() > 0) ? true : false;

                        #region History
                        if (success)
                        {
                            appDbContext.Histories.Attach(history);
                            appDbContext.Entry(history).State = EntityState.Added;
                            success = (appDbContext.SaveChanges() > 0) ? true : false;

                            if (success)
                            {
                                appDbContext.HistoryUsers.Attach(historyUser);
                                appDbContext.Entry(historyUser).State = EntityState.Added;
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
                    #endregion COMMIT OR ROLLBACK TRANSACTION
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
        #endregion User

        #region Rol
        /// <summary>
        /// Obtiene el Rol por su ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Role GetRoleById(string id)
        {
            try
            {
                var rol = appDbContext.Roles
                                    .Where(x => x.Id == id)
                                    .FirstOrDefault();
                return rol;
            }
            catch (Exception ex)
            {
                logger.Error($"[{TAG}] - {ex}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene el Rol por su código
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public Role GetRoleByCode(string code)
        {
            try
            {
                var rol = appDbContext.Roles
                                    .Where(x => x.Code == code)
                                    .FirstOrDefault();
                return rol;
            }
            catch (Exception ex)
            {
                logger.Error($"[{TAG}] - {ex}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene listado de roles
        /// </summary>
        /// <param name="activo"></param>
        /// <returns></returns>
        public List<Role> GetRoles(bool? active)
        {
            try
            {
                #region Expresión Activo
                Expression<Func<Role, bool>> activeExpression;
                if (active.HasValue)
                {
                    activeExpression = x => x.Active == active.Value;
                }
                else
                {
                    activeExpression = x => x.Active == true || x.Active == false;
                }
                #endregion Expresión Activo

                var list = appDbContext.Roles
                                            .Where(activeExpression)
                                            .OrderBy(x => x.Name)
                                            .ToList();

                return list;
            }
            catch (Exception e)
            {
                logger.Error($"{TAG} -- {e}");
                throw;
            }
        }
        #endregion Rol
    }
}
