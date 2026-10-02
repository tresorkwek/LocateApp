using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Repository;
using LocateApp.Models;
using LocateApp.DataTransferObjects;

namespace LocateApp.Controllers
{
    public static class ClaimControllers
    {
        public static List<string> GetClaims(string claim = null)
        {
            string sql = claim == null ? SqlClaim.SelectAll : SqlClaim.SelectByName;
            var claimValue = claim == null ? null : new { Claim = claim };

            return SqlDataAccess.SelectData<string>(sql, claimValue);
        }

        public static List<string> GetAgentClaims()
        {
            return SqlDataAccess.SelectData<string>(SqlClaim.SelectForAgent);
        }

        public static List<string> GetClaimsForProfil(int idProfil, bool isExternUser)
        {
            string sql = isExternUser ? SqlClaim.SelectByProfil : SqlClaim.SelectForAgentByProfil;

            return SqlDataAccess.SelectData<string>(sql, new { IdProfil = idProfil });
        }


        public static List<UserClaim> GetClaimsByProfilForConfig(int idProfil)
        {         
            return SqlDataAccess.SelectData<UserClaim>(SqlClaim.SelectByProfilForConfig, new { IdProfil = idProfil });
        }

        public static bool UpSetClaim(AddUpdateClaimRequest addClaimRequest, Identity user)
        {
            List<string> idMenuList = addClaimRequest.IdMenu.Split(',').ToList();
            //List<string> customUrl = addClaimRequest.CustomUrl.Split(',').ToList();

            List<(string, object)> sqlAndData = new List<(string, object)>();
            sqlAndData.Add((SqlClaim.Delete, new { IdProfil = addClaimRequest.IdProfil }));

            foreach (string idMenu in idMenuList)
            {
                sqlAndData.Add((SqlClaim.Insert, new { IdProfil = addClaimRequest.IdProfil, IdMenu = int.Parse(idMenu) }));
            }
            int nbreOfRow = SqlDataAccess.SaveDataWithTransaction(sqlAndData, user);

            return nbreOfRow > 0;
        }

    }
}