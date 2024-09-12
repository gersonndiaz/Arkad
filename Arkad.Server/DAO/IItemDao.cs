using Arkad.Server.Models;

namespace Arkad.Server.DAO
{
    interface IItemDao
    {
        #region Item
        Item GetById(string id);
        Item GetByName(string name);
        List<Item> GetAll(bool? active);
        bool Save(Item item, History history, HistoryItem historyItem);
        bool Update(Item item, History history, HistoryItem historyItem);
        bool Update(List<Item> items, List<History> histories, List<HistoryItem> historiesItem);
        #endregion Item

        #region Historial
        List<HistoryItem> FindHistories(int limit, int offset, Item item, string search, bool? activo);
        int CountHistories(Item item, string search, bool? activo);
        #endregion Historial
    }
}
