using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Repository;

namespace LocateApp.Controllers
{
    public static class ModuleController
    {
        public static List<Module> GetModules(int? idModule = null)
        {
            string sql = idModule == null ? SqlModule.SelectAll : SqlModule.SelectById;
            var moduleValue = idModule == null ? null : new { IdModule = idModule };

            return SqlDataAccess.SelectData<Module>(sql, moduleValue);
        }
    }
}