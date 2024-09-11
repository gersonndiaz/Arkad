using Arkad.Server.Models;
using Arkad.Server.Models._Context;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Linq.Expressions;

namespace Arkad.Server.DAO.Impl
{
    public class UsuarioDao : IUsuarioDao
    {
        private static string TAG = typeof(UsuarioDao).FullName;
        private Logger logger = LogManager.GetCurrentClassLogger();
        private AppDbContext appDbContext;

        #region Contructor
        /// <summary>
        /// Constructor por defecto, el cual tiene las configuraciones estándar para el acceso a datos
        /// </summary>
        /// <param name="appDbContext"></param>
        public UsuarioDao()
        {
            this.appDbContext = (appDbContext != null) ? appDbContext : new AppDbContext();
        }

        /// <summary>
        /// Constructor que recibe el contexto
        /// </summary>
        /// <param name="appDbContext"></param>
        public UsuarioDao(AppDbContext appDbContext)
        {
            this.appDbContext = (appDbContext != null) ? appDbContext : new AppDbContext();
        }
        #endregion Constructor

        #region Usuario
        /// <summary>
        /// Obtiene un usuario por su ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Usuario GetById(string id)
        {
            try
            {
                var usuario = appDbContext.Usuarios
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
        /// Obtiene usuario por su RUN
        /// </summary>
        /// <param name="run"></param>
        /// <returns></returns>
        public Usuario GetByRun(string run)
        {
            try
            {
                var usuario = appDbContext.Usuarios
                                    .Where(x => x.Run.ToUpper() == run.ToUpper())
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
        /// Obtiene usuario por su Correo
        /// </summary>
        /// <param name="correo"></param>
        /// <returns></returns>
        public Usuario GetByCorreo(string correo)
        {
            try
            {
                var usuario = appDbContext.Usuarios
                                    .Where(x => x.Correo.ToUpper() == correo.ToUpper())
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
        /// Obtiene listado de usuarios encontrados
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="search"></param>
        /// <param name="activo"></param>
        /// <returns></returns>
        public List<Usuario> GetAll(int limit, int offset, string search, bool? activo)
        {
            try
            {
                #region Expresión Search
                Expression<Func<Usuario, bool>> searchExpression;
                if (!String.IsNullOrEmpty(search))
                {
                    searchExpression = x => (x.Run.ToUpper().Contains(search.ToUpper())
                                                || x.Nombre.ToUpper().Contains(search.ToUpper())
                                                || x.Correo.ToUpper().Contains(search.ToUpper()));
                }
                else
                {
                    searchExpression = x => x.Run != null;
                }
                #endregion Expresión Search

                #region Expresión Activo
                Expression<Func<Usuario, bool>> activeExpression;
                if (activo.HasValue)
                {
                    activeExpression = x => x.BActivo == activo.Value;
                }
                else
                {
                    activeExpression = x => x.BActivo == true || x.BActivo == false;
                }
                #endregion Expresión Activo

                var list = appDbContext.Usuarios
                                            .Where(searchExpression)
                                            .Where(activeExpression)
                                            .OrderBy(x => x.Nombre)
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
        /// Obtiene la cantidad total de registros encontrados
        /// </summary>
        /// <param name="search"></param>
        /// <param name="activo"></param>
        /// <returns></returns>
        public int Count(string search, bool? activo)
        {
            try
            {
                #region Expresión Search
                Expression<Func<Usuario, bool>> searchExpression;
                if (!String.IsNullOrEmpty(search))
                {
                    searchExpression = x => (x.Run.ToUpper().Contains(search.ToUpper())
                                                || x.Nombre.ToUpper().Contains(search.ToUpper())
                                                || x.Correo.ToUpper().Contains(search.ToUpper()));
                }
                else
                {
                    searchExpression = x => x.Run != null;
                }
                #endregion Expresión Search

                #region Expresión Activo
                Expression<Func<Usuario, bool>> activeExpression;
                if (activo.HasValue)
                {
                    activeExpression = x => x.BActivo == activo.Value;
                }
                else
                {
                    activeExpression = x => x.BActivo == true || x.BActivo == false;
                }
                #endregion Expresión Activo

                int count = appDbContext.Usuarios
                                            .Where(searchExpression)
                                            .Where(activeExpression)
                                            .OrderBy(x => x.Nombre)
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
        /// Actualiza los datos de usuario
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public bool Update(Usuario usuario)
        {
            bool success = false;

            try
            {
                #region TRANSACCION
                using (var dbContextTransaction = appDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        appDbContext.Usuarios.Attach(usuario);
                        appDbContext.Entry(usuario).State = EntityState.Modified;
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
        /// Actualiza la información de usuario
        /// </summary>
        /// <param name="usuario"></param>
        /// <param name="historial"></param>
        /// <returns></returns>
        public bool Update(Usuario usuario, HistorialUsuario historial)
        {
            bool success = false;

            try
            {
                #region TRANSACCION
                using (var dbContextTransaction = appDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        #region Usuario
                        appDbContext.Usuarios.Attach(usuario);
                        appDbContext.Entry(usuario).State = EntityState.Modified;
                        success = (appDbContext.SaveChanges() > 0) ? true : false;
                        #endregion Usuario

                        #region Historial
                        if (success)
                        {
                            appDbContext.HistorialUsuario.Attach(historial);
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
        /// Registra un nuevo usuario
        /// </summary>
        /// <param name="usuario"></param>
        /// <param name="historial"></param>
        /// <returns></returns>
        public bool Save(Usuario usuario, HistorialUsuario historial)
        {
            bool success = false;

            try
            {
                #region TRANSACCION
                using (var dbContextTransaction = appDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        #region Usuario
                        appDbContext.Usuarios.Attach(usuario);
                        appDbContext.Entry(usuario).State = EntityState.Added;
                        success = (appDbContext.SaveChanges() > 0) ? true : false;
                        #endregion Usuario

                        #region Historial
                        if (success)
                        {
                            appDbContext.HistorialUsuario.Attach(historial);
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
        #endregion Usuario

        #region Rol
        /// <summary>
        /// Obtiene el Rol por su ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Rol GetRolById(string id)
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
        /// <param name="codigo"></param>
        /// <returns></returns>
        public Rol GetRolByCodigo(string codigo)
        {
            try
            {
                var rol = appDbContext.Roles
                                    .Where(x => x.Codigo == codigo)
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
        public List<Rol> GetRoles(bool? activo)
        {
            try
            {
                #region Expresión Activo
                Expression<Func<Rol, bool>> activeExpression;
                if (activo.HasValue)
                {
                    activeExpression = x => x.BActivo == activo.Value;
                }
                else
                {
                    activeExpression = x => x.BActivo == true || x.BActivo == false;
                }
                #endregion Expresión Activo

                var list = appDbContext.Roles
                                            .Where(activeExpression)
                                            .OrderBy(x => x.Nombre)
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

        #region Historial
        /// <summary>
        /// Obtiene listado de registros encontrados
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="usuario"></param>
        /// <param name="search"></param>
        /// <param name="activo"></param>
        /// <returns></returns>
        public List<HistorialUsuario> FindHistorial(int limit, int offset, Usuario usuario, string search, bool? activo)
        {
            try
            {
                #region Expresión Search
                Expression<Func<HistorialUsuario, bool>> searchExpression;
                if (!String.IsNullOrEmpty(search))
                {
                    searchExpression = x => x.Descripcion.ToUpper().Contains(search);
                }
                else
                {
                    searchExpression = x => x.Descripcion != null || x.Descripcion == null;
                }
                #endregion Expresión Search

                var list = appDbContext.HistorialUsuario
                                            .Where(x => x.UsuarioActualId == usuario.Id)
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
        /// <param name="usuario"></param>
        /// <param name="search"></param>
        /// <param name="activo"></param>
        /// <returns></returns>
        public int CountHistorial(Usuario usuario, string search, bool? activo)
        {
            try
            {
                #region Expresión Search
                Expression<Func<HistorialUsuario, bool>> searchExpression;
                if (!String.IsNullOrEmpty(search))
                {
                    searchExpression = x => x.Descripcion.ToUpper().Contains(search);
                }
                else
                {
                    searchExpression = x => x.Descripcion != null || x.Descripcion == null;
                }
                #endregion Expresión Search

                int count = appDbContext.HistorialUsuario
                                            .Where(x => x.UsuarioActualId == usuario.Id)
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
