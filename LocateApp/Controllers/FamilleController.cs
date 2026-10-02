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
    public static class FamilleController
    {
        public static List<Famille> Select(string Nom = null, bool fetchAll = false)
        {
            string sql;

            if (fetchAll)
            {
                sql = Nom == null ? SqlFamille.SelectAll : SqlFamille.SelectByName;
            }
            else
            {
                sql = Nom == null ? SqlFamille.SelectAllActive : SqlFamille.SelectActiveByName;
            }

            if(Nom != null)
            {
                Nom += "%";
            }

            return SqlDataAccess.SelectData<Famille>(sql, new { Nom });
        }

        public static List<Famille> SelectById(long Id)
        {
            return SqlDataAccess.SelectData<Famille>(SqlFamille.SelectById, new { Id });
        }

        public static List<Famille> SelectIdentifie()
        {
            return StatCache.Get("famille:identifie", () => SqlDataAccess.SelectData<Famille>(SqlFamille.SelectAllIdentifie, commandTimeout: StatCache.TimeoutStatistiques));
        }

        public static List<Famille> SelectWithNumber(string Etat = null)
        {
            string sql = Etat == null ? SqlFamille.SelectWithNumber : SqlFamille.SelectWithNumberByEtat;

            return StatCache.Get("famille:nombre:" + (Etat ?? "tous"), () => SqlDataAccess.SelectData<Famille>(sql, new { Etat }, commandTimeout: StatCache.TimeoutStatistiques));
        }

        public static List<FamillieStatPieRequest> SelectStatPie()
        {
            return StatCache.Get("famille:pie", () => SqlDataAccess.SelectData<FamillieStatPieRequest>(SqlFamille.SelectAllStatPie, commandTimeout: StatCache.TimeoutStatistiques));
        }

        public static List<FamillieStatPieLegendRequest> SelectStatPieLegend()
        {
            return StatCache.Get("famille:pie:legende", () => SqlDataAccess.SelectData<FamillieStatPieLegendRequest>(SqlFamille.SelectAllStatPieLegend, commandTimeout: StatCache.TimeoutStatistiques));
        }
        public static List<FamillieStatRadardRequest> SelectStatRadar()
        {
            return StatCache.Get("famille:radar", () => SqlDataAccess.SelectData<FamillieStatRadardRequest>(SqlFamille.SelectAllStatRadar, commandTimeout: StatCache.TimeoutStatistiques));
        }

        public static bool Insert(AddFamilleRequest insertFamilleValues, Identity user)
        {
            insertFamilleValues.UserCreation = user.UserName;

            int nbreRow = SqlDataAccess.SaveData(SqlFamille.Insert, insertFamilleValues, user);
            return nbreRow > 0;
        }

        public static bool Update(ModifyFamillieRequest modifyFamilleValues, Identity user)
        {
            modifyFamilleValues.UserCreation = user.UserName;
            modifyFamilleValues.DateCreation = DateTime.Today;

            int nbreRow = SqlDataAccess.SaveData(SqlFamille.Update, modifyFamilleValues, user);
            return nbreRow > 0;
        }
    }
}