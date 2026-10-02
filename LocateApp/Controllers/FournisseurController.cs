using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.DataTransferObjects;
using LocateApp.Models;
using LocateApp.Repository;

namespace LocateApp.Controllers
{
    public static class FournisseurController
    {
        public static List<Fournisseur> Select(string Nom = null, bool fetchAll=false)
        {
            string sql;

            if (fetchAll)
            {
                sql = Nom == null ? SqlFournisseur.SelectAll : SqlFournisseur.SelectByName;
            }
            else
            {
                sql = Nom == null ? SqlFournisseur.SelectAllActive : SqlFournisseur.SelectActiveByName;
            }

            return SqlDataAccess.SelectData<Fournisseur>(sql, new { Nom });
        }

        public static List<Fournisseur> SelectById(long Id)
        {
            return SqlDataAccess.SelectData<Fournisseur>(SqlFournisseur.SelectById, new { Id });
        }

        public static bool Insert(AddFournisseurRequest insertFournisseurValues, Identity user)
        {
            insertFournisseurValues.UserCreation = user.UserName;

            int nbreRow = SqlDataAccess.SaveData(SqlFournisseur.Insert, insertFournisseurValues, user);
            return nbreRow > 0;
        }

        public static bool Update(ModifyFournisseurRequest modifyFournisseurValues, Identity user)
        {
            modifyFournisseurValues.UserCreation = user.UserName;
            modifyFournisseurValues.DateCreation = DateTime.Today;

            int nbreRow = SqlDataAccess.SaveData(SqlFournisseur.Update, modifyFournisseurValues, user);
            return nbreRow > 0;
        }

    }
}