using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.DataTransferObjects;
using LocateApp.Repository;

namespace LocateApp.Controllers
{
    public static class MenuToShowController
    {
        public static List<MenuToShow> GetMenuToShows(int? idProfil)
        {
            string sql = idProfil == null ? SqlMenuToShow.SelectAll : SqlMenuToShow.SelectByProfil;

            return SqlDataAccess.SelectData<MenuToShow>(sql, idProfil== null? null : new { IdProfil = idProfil });
        }
    }
}