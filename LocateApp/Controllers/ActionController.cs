using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Repository;

namespace LocateApp.Controllers
{
    public static class ActionController
    {
        public static List<Actions> GetAction(int? idAction = null)
        {
            string sql = idAction == null ? SqlAction.SelectAll : SqlAction.SelectById;

            return SqlDataAccess.SelectData<Actions>(sql, new { IdAction = idAction });
        }
    }
}