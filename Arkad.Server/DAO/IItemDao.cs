using Arkad.Server.Models;

namespace Arkad.Server.DAO
{
    interface IItemDao
    {
        #region Item
        Item GetById(string id);
        Item GetByNombre(string nombre);
        List<Item> GetAll(bool? activo);
        bool Save(Item item, HistorialItem historial);
        bool Update(Item item, HistorialItem historial);
        bool Update(List<Item> items, List<HistorialItem> historial);
        #endregion Item

        #region Historial
        List<HistorialItem> FindHistorial(int limit, int offset, Item item, string search, bool? activo);
        int CountHistorial(Item item, string search, bool? activo);
        #endregion Historial
    }
}
