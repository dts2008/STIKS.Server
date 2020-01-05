using STIKS.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Dapper;

namespace STIKS.DBManager
{
    public class UserManager : CommonDBManager<UserInfo>
    {
        public static UserManager Instance { get; } = new UserManager();
        public UserInfo ByLogin(string email)
        {
            UserInfo result = null;

            if (!DBCommand((IDbConnection db) =>
            {
                result = db.QueryFirstOrDefault<UserInfo>($"select * from {typeName} where Email = @Email limit 1", new { Email = email });
            }
            ))
                return null;

            return result;
        }
    }
}
