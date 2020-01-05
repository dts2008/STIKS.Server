using Dapper;
using MySql.Data.MySqlClient;
using STIKS.Common;
using STIKS.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Dapper.Contrib.Extensions;
using System.Linq;

namespace STIKS.DBManager
{
    public class CommonDBManager<T> : IManager where T : CommonInfo
    {
        #region Field(s)

        public static string typeName;

        public PropertyInfo[] typeProperties;

        private static string connectionString;

        #endregion

        #region Constructor(s)

        static CommonDBManager()
        {
            connectionString = AppSettings.Instance.GetSection("ConnectionStrings").GetValue<string>("MySQLConnection");
        }

        public CommonDBManager()
        {
            typeName = typeof(T).Name.ToLower();

            var t = typeof(T);
            typeProperties = t.GetProperties();

            Init();
        }

        #endregion

        public virtual void Init()
        {
        }

        public virtual bool Delete(int id)
        {
            return DBCommand((IDbConnection db) =>
            {
                db.Execute($"delete from {typeName} where id = {id}");
            }
            );
        }

        public virtual bool Update(CommonInfo info)
        {
            T ci = info as T;

            if (!DBCommand((IDbConnection db) => {
                db.Update(ci);
            })) return false;

            return true;
        }

        public virtual bool Insert(CommonInfo info)
        {
            long result = 0;
            T ci = info as T;

            if (!DBCommand((IDbConnection db) => {
                result = db.Insert(ci);
            })) return false;

            ci.Id = (int)result;

            return true;
        }

        public virtual CommonInfo Get(int id)
        {
            CommonInfo result = null;

            DBCommand((IDbConnection db) =>
            {
                result = db.QueryFirstOrDefault<T>($"select * from {typeName} where id = {id} limit 1");
            }
            );

            return result;
        }

        public virtual CommonInfo Get(string field, object value)
        {
            CommonInfo result = null;

            DBCommand((IDbConnection db) =>
            {
                result = db.QueryFirstOrDefault<T>($"select * from {typeName} where {field} = '{value}' limit 1");
            }
            );

            return result;
        }

        public virtual List<CommonInfo> Get(UserItem userItem, int page, int pageSize, out int total_items, string sort_by, bool descending, List<FilterItem> filterList)
        {
            total_items = 0;
            string desc = descending ? "DESC" : string.Empty;
            var where = new StringBuilder();

            if (filterList != null && filterList.Count > 0)
            {

                foreach (var filters in filterList)
                {
                    var field = typeProperties.FirstOrDefault(f => f.Name == filters.Name);
                    if (field == null) continue;

                    if (where.Length > 0) where.Append("AND ");
                    where.Append($"{filters.Name} {GetFilterOperator(filters.fType)} {filters.Value}");

                    GetFilterOperator(filters.fType);
                }

                if (where.Length > 0) where.Insert(0, " WHERE ");
            }

            if (string.IsNullOrEmpty(sort_by)) sort_by = "id";

            string query = pageSize != -1 ?
                $"SELECT * FROM {typeName} {where.ToString()} ORDER BY {sort_by} {desc} LIMIT {(page - 1) * pageSize}, {pageSize} " :
                $"SELECT * FROM {typeName} {where.ToString()} ORDER BY {sort_by} {desc} ";

            IEnumerable<T> result = null;
            int result_count = 0;

            if (!DBCommand((IDbConnection db) => {
                result = db.Query<T>(query);
                result_count = db.ExecuteScalar<int>($"SELECT COUNT(*) FROM {typeName} {where.ToString()}");
            })) return null;

            total_items = result_count;
            return new List<CommonInfo>(result);
        }

        #region Private method(s)


        #region Compare Methods

        protected static bool DBCommand(Action<IDbConnection> action)
        {
            try
            {
                using (IDbConnection db = new MySqlConnection(connectionString))
                {
                    db.Open();

                    action(db);
                }

                return true;
            }
            catch (Exception exc)
            {
                Logger.Instance.Save(exc);
            }

            return false;
        }

        private static string GetFilterOperator(FilterType filterType)
        {
            switch (filterType)
            {
                case FilterType.Equal:
                    return " = ";
                case FilterType.MoreOrEqual:
                    return " >= ";
                case FilterType.LessOrEqual:
                    return " <= ";
                case FilterType.More:
                    return " > ";
                case FilterType.Less:
                    return " < ";
                case FilterType.In:
                    return " in ";
                default:
                    return "";
            }
        }

        #endregion Compare Methods

        #endregion
    }
}
