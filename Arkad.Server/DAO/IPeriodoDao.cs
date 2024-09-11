using Arkad.Server.Models;

namespace Arkad.Server.DAO
{
    interface IPeriodoDao
    {
        #region Periodo
        Periodo GetById(string id);
        Periodo GetCurrent();
        Periodo GetByAnioMes(int anio, int mes);
        List<Periodo> GetAll(bool? activo);
        List<Periodo> GetAll(int limit, int offset, string search, bool? activo);
        int Count(string search, bool? activo);
        bool Save(Periodo periodo, HistorialPeriodo historial);
        bool Update(Periodo periodo, HistorialPeriodo historial);
        bool Update(Periodo periodo, HistorialPeriodo historial, Periodo periodoPrev, HistorialPeriodo historialPrev);
        #endregion Periodo

        #region Historial
        List<HistorialPeriodo> FindHistorial(int limit, int offset, Periodo periodo, string search, bool? activo);
        int CountHistorial(Periodo periodo, string search, bool? activo);
        #endregion Historial
    }
}
