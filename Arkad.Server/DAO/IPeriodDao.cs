using Arkad.Server.Models;

namespace Arkad.Server.DAO
{
    interface IPeriodDao
    {
        #region Period
        Period GetById(string id);
        Period GetCurrent();
        Period GetByYearMonth(int year, int month);
        List<Period> GetAll(bool? active);
        List<Period> GetAll(int limit, int offset, string search, bool? active);
        int Count(string search, bool? active);
        bool Save(Period period, History history, HistoryPeriod historyPeriod);
        bool Update(Period period, History history, HistoryPeriod historyPeriod);
        bool Update(Period period, History history, HistoryPeriod historyPeriod, Period periodPrev, History historyPrev, HistoryPeriod historyPeriodPrev);
        #endregion Period

        #region Historial
        List<HistoryPeriod> FindHistory(int limit, int offset, Period period, string search, bool? active);
        int CountHistory(Period period, string search, bool? active);
        #endregion Historial
    }
}
