using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Repository;

namespace LocateApp.Controllers
{
    public static class ModeleController
    {
        public static List<Modele> Select(string Designation = null, bool fetchAll=false)
        {
            string sql;

            if (fetchAll)
            {
                sql = Designation == null ? SqlModele.SelectAll : SqlModele.SelectByName;
            }
            else
            {
                sql = Designation == null ? SqlModele.SelectAllActive : SqlModele.SelectActiveByName;
            }
             

            return SqlDataAccess.SelectData<Modele>(sql, new { Designation });
        }

        public static Modele SelectById(int Id)
        {
            return SqlDataAccess.SelectData<Modele>(SqlModele.SelectById, new { Id }).FirstOrDefault();
        }
    }
}