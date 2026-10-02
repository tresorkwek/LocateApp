using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.DataTransferObjects;
using LocateApp.Models;
using LocateApp.Repository;

namespace LocateApp.Controllers
{
    public static class OrganeTypeController
    {
        public static List<OrganeType> Select(int? Id = null)
        {
            string sql = Id == null ? SqlOrganeType.SelectAll : SqlOrganeType.SelectById;
            return SqlDataAccess.SelectData<OrganeType>(sql, new { Id });
        }

        public static bool Insert(InsertOrganeTypeRequest addOrganeTypeValues, Identity user)
        {
            int nbreRow = SqlDataAccess.SaveData(SqlOrganeType.Insert, addOrganeTypeValues, user);

            return nbreRow > 0;
        }

        public static bool Update(ModifyOrganeTypeRequest modifyOrganeTypeRequest, Identity user)
        {
            int nbreRow = SqlDataAccess.SaveData(SqlOrganeType.Update, modifyOrganeTypeRequest, user);

            return nbreRow > 0;
        }

    }
}