﻿using Arkad.Server.Models;

namespace Arkad.Server.DAO
{
    interface IUserDao
    {
        #region User
        User GetById(string id);
        User GetByEmail(string email);
        bool Update(User user);
        #endregion User

        #region Role
        Role GetRoleById(string id);
        Role GetRoleByCode(string code);
        List<Role> GetRoles(bool? active);
        #endregion Role
    }
}
