using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Repository;

namespace LocateApp.Controllers
{
    public static class GroupMenuController
    {
        public static List<GroupMenu> GetGroupMenu(int? idGroupMenu = null)
        {
            string sql = idGroupMenu == null ? SqlGroupMenu.SelectAllVisible : SqlGroupMenu.SelectById;
            var groupMenuValue = idGroupMenu == null ? null : new { IdGroupMenu = idGroupMenu };

            return SqlDataAccess.SelectData<GroupMenu>(sql, groupMenuValue);
        }
        public static List<GroupMenu> GetGroupMenuByProfil(Profil profil)
        {
            string sql = SqlGroupMenu.SelectAllVisible;

            if (!profil.Root)
            {
                sql = profil.Externe ? SqlGroupMenu.SelectByProfil : SqlGroupMenu.SelectAgentByProfil;
            }

            return SqlDataAccess.SelectData<GroupMenu>(sql, new { IdProfil = profil.IdProfil });
        }

        public static List<GroupMenu> GetAllGroupMenu(int? idGroupMenu = null)
        {
            string sql = idGroupMenu == null ? SqlGroupMenu.SelectAll : SqlGroupMenu.SelectById;
            var groupMenuValue = idGroupMenu == null ? null : new { IdGroupMenu = idGroupMenu };

            return SqlDataAccess.SelectData<GroupMenu>(sql, groupMenuValue);
        }

        public static bool Visible(int idGroupMenu, bool visible)
        {
            string sql = visible ? SqlGroupMenu.MakeVisible : SqlGroupMenu.MakeInVisible;
            int value = SqlDataAccess.SaveData(sql, new { IdGroupMenu = idGroupMenu });

            return value > 0;
        }

    }
}