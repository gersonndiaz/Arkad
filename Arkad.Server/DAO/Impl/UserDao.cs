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
        /// <param name="correo"></param>
        /// <returns></returns>
        public User GetByEmail(string email)
        {
            try
            {
                var usuario = appDbContext.Users
                                    .Where(x => x.Email.ToUpper() == correo.ToUpper())
                                    .FirstOrDefault();
                return usuario;
            }
            catch (Exception ex)
            {
                logger.Error($"[{TAG}] - {ex}");
                throw;
            }
        }
        #endregion User
    }
}
