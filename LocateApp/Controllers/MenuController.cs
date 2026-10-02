using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.DataTransferObjects;
using LocateApp.Models;
using LocateApp.Repository;
using LocateApp.Utilities;

namespace LocateApp.Controllers
{
    public static class MenuController
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(MenuController));

        public static List<Menu> GetMenu(int? idMenu = null)
        {
            string sql = idMenu == null ? SqlMenu.SelectAllVisible : SqlMenu.SelectById;
            var menuValue = idMenu == null ? null : new { IdMenu = idMenu };
            return SqlDataAccess.SelectData<Menu>(sql, menuValue);
        }
        public static List<Menu> GetMenuByGroupMenu(int idGroupMenu)
        {
            return SqlDataAccess.SelectData<Menu>(SqlMenu.SelectByGroupMenu, new { IdGroupMenu = idGroupMenu });
        }

        public static Menu GetMenuByName(string nom)
        {
            return SqlDataAccess.SelectData<Menu>(SqlMenu.SelectByName, new { Nom = nom }).FirstOrDefault();
        }

        public static List<Menu> GetAllMenu(int? idMenu = null)
        {
            string sql = idMenu == null ? SqlMenu.SelectAll : SqlMenu.SelectById;
            var menuValue = idMenu == null ? null : new { IdMenu = idMenu };
            return SqlDataAccess.SelectData<Menu>(sql, menuValue);
        }
        public static List<Menu> GetAllMenuByGroupMenu(int idGroupMenu)
        {
            return SqlDataAccess.SelectData<Menu>(SqlMenu.SelectAllByGroupMenu, new { IdGroupMenu = idGroupMenu });
        }

        public static bool AddMenu(AddMenuRequest addMenuRequest, Identity user)
        {
            int nbreRow = SqlDataAccess.SaveData(SqlMenu.Insert, addMenuRequest, user);

            return nbreRow > 0;
        }

        public static bool ModifyMenu(ModifyMenuRequest modifyMenuRequest, Identity user)
        {
            int nbreRow = SqlDataAccess.SaveData(SqlMenu.Update, modifyMenuRequest, user);             

            return nbreRow > 0;
        }

    }
}