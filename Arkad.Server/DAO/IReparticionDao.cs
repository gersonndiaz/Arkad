using Arkad.Server.Models;

namespace Arkad.Server.DAO
{
    interface IReparticionDao
    {
        #region Reparticion
        Reparticion GetById(string id);
        Reparticion GetByCodigo(string codigo);
        Reparticion GetByNombre(string nombre);
        List<Reparticion> GetAll(bool? activo);
        bool Save(Reparticion reparticion, HistorialReparticion historial);
        bool Update(Reparticion reparticion, HistorialReparticion historial);
        #endregion Reparticion

        #region Historial
        List<HistorialReparticion> FindHistorial(int limit, int offset, Reparticion reparticion, string search, bool? activo);
        int CountHistorial(Reparticion reparticion, string search, bool? activo);
        #endregion Historial
    }
}
