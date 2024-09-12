using Arkad.Server.Models;

namespace Arkad.Server.DAO
{
    interface IUserDao
    {
        #region User
        User GetById(string id);
        User GetByEmail(string email);
        #endregion User
    }
}
