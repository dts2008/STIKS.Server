using STIKS.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace STIKS.DBManager
{
    public interface IManager
    {
        bool Delete(int id);

        bool Insert(CommonInfo info);

        bool Update(CommonInfo info);

        CommonInfo Get(int id);

        CommonInfo Get(string field, object value);

    }
}
