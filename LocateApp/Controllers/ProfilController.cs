using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Repository;
using LocateApp.DataTransferObjects;

namespace LocateApp.Controllers
{
    public static class ProfilController
    {
        public static List<Profil> GetProfil(int? idProfil = null)
        {
            string sql = SqlProfil.SelectAll;
            ProfilSelectRequest selectProfil = null;

            if (idProfil != null)
            {
                sql = SqlProfil.SelectById;
                selectProfil = new ProfilSelectRequest() { IdProfil = (int)idProfil };
            }

            return SqlDataAccess.SelectData<Profil>(sql, selectProfil);
        }

        public static Profil GetDefaultProfil()
        {
            return SqlDataAccess.SelectData<Profil>(SqlProfil.SelectDefault).FirstOrDefault();
        }

        public static bool AddProfil(AddProfilRequest addPorfilRequest, Identity user)
        {
            int nbreRow = SqlDataAccess.SaveData(SqlProfil.Insert, addPorfilRequest, user);

            return nbreRow > 0;
        }

        public static bool ModifyProfil(ModifyProfilRequest modifyProfilRequest, Identity user)
        {
            int nbreRow = SqlDataAccess.SaveData(SqlProfil.Update, modifyProfilRequest, user);

            return nbreRow > 0;
        }

        public static List<Profil> GetProfilWithoutClaims()
        {
            return SqlDataAccess.SelectData<Profil>(SqlProfil.SelectAllWithoutClaims);
        }
        public static List<Profil> GetProfilWithClaims(int? idProfil)
        {
            string sql = idProfil == null ? SqlProfil.SelectAllWithClaims : SqlProfil.SelectAllWithClaimsByProfil;
            var values = idProfil == null ? null : new { IdProfil = idProfil };

            return SqlDataAccess.SelectData<Profil>(sql, values);
        }

    }
}