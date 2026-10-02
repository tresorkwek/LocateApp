using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.DataTransferObjects;
using LocateApp.Models;
using LocateApp.Repository;

namespace LocateApp.Controllers
{
    public static class CategorieController
    {
        public static List<Categorie> Select(string Designation = null, bool fetchAll=false)
        {
            string sql;

            if (fetchAll)
            {
                sql = Designation == null ? SqlCategorie.SelectAll : SqlCategorie.SelectByName;
            }
            else
            {
                sql = Designation == null ? SqlCategorie.SelectAllActive : SqlCategorie.SelectActiveByName;
            }             

            return SqlDataAccess.SelectData<Categorie>(sql, new { Designation });
        }

        public static List<Categorie> SelectById(long Id)
        {
            return SqlDataAccess.SelectData<Categorie>(SqlCategorie.SelectById, new { Id });
        }


        public static bool Insert(AddCategorieRequest AddCategorieValues, Identity user)
        {
            AddCategorieValues.UserCreation = user.UserName;
            int nbreRow = SqlDataAccess.SaveData(SqlCategorie.Insert, AddCategorieValues, user);
            return nbreRow > 0;
        }

        public static bool Update(ModifyCategorieRequest modifyCategorieValues, Identity user)
        {
            modifyCategorieValues.UserCreation = user.UserName;
            modifyCategorieValues.DateCreation = DateTime.Today;

            int nbreRow = SqlDataAccess.SaveData(SqlCategorie.Update, modifyCategorieValues, user);
            return nbreRow > 0;
        }

        public static bool Delete(long Id, Identity user)
        {
            int nbreRow = SqlDataAccess.SaveData(SqlCategorie.Delete, new { Id }, user);
            return nbreRow > 0;
        }

    }
}