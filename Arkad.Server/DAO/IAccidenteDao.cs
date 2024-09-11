using Arkad.Server.Models;

namespace Arkad.Server.DAO
{
    interface IAccidenteDao
    {
        #region Accidente
        Accidente GetById(string id);
        Accidente GetCurrent(Periodo periodo, Reparticion reparticion, Item item, string tipo);
        List<Accidente> GetAll(Periodo periodo, Reparticion reparticion, bool? activo);
        bool Save(List<Accidente> accidentes, List<Accidente> accidentesMod, HistorialPeriodoAccidentes historial);
        bool Save(List<Accidente> accidentes, List<Accidente> accidentesMod, IndicadorPeriodo indicadorPeriodo, HistorialPeriodoAccidentes historial);
        #endregion Accidente

        #region Indicador
        IndicadorPeriodo GetIndicadorById(string id);
        IndicadorPeriodo GetIndicador(Periodo periodo, Reparticion reparticion);
        bool UpdateIndicador(IndicadorPeriodo indicadorPeriodo, HistorialPeriodoAccidentes historial);
        #endregion Indicador
    }
}
