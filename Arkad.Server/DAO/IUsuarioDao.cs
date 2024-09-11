using Arkad.Server.Models;

namespace Arkad.Server.DAO
{
    interface IUsuarioDao
    {
        #region Usuario
        Usuario GetById(string id);
        Usuario GetByRun(string run);
        Usuario GetByCorreo(string correo);
        List<Usuario> GetAll(int limit, int offset, string search, bool? activo);
        int Count(string search, bool? activo);
        bool Update(Usuario usuario);
        bool Update(Usuario usuario, HistorialUsuario historial);
        bool Save(Usuario usuario, HistorialUsuario historial);
        #endregion Usuario

        #region Rol
        Rol GetRolById(string id);
        Rol GetRolByCodigo(string codigo);
        List<Rol> GetRoles(bool? activo);
        #endregion Rol

        #region Historial
        List<HistorialUsuario> FindHistorial(int limit, int offset, Usuario usuario, string search, bool? activo);
        int CountHistorial(Usuario usuario, string search, bool? activo);
        #endregion Historial
    }
}
