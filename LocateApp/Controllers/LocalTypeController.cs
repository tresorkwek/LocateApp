using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Repository;

namespace LocateApp.Controllers
{
    public static class LocalTypeController
    {
        public static List<LocalType> Select(int? id = null)
        {
            string sql = id == null ? SqlLocalType.SelectAll : SqlLocalType.SelectById;

            return SqlDataAccess.SelectData<LocalType>(sql, new { Id = id });
        }
    }
}