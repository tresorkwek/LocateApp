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
    public static class ObservationsController
    {
        public static List<Observations> Select(int? Id = null)
        {
            string sql = Id == null ? SqlObservations.SelectAll : SqlObservations.SelectById;

            return SqlDataAccess.SelectData<Observations>(sql, new { Id });
        }

        public static List<Observations> SelectByEtat(string Etat)
        {
            return SqlDataAccess.SelectData<Observations>(SqlObservations.SelectByEtat, new { Etat });
        }
        /// <param name="delaiLong">Vrai pour les appels en arrière-plan (graphiques) : délai SQL StatCommandTimeout au lieu de 30 s.</param>
        public static List<SelectStatObservationRequest> SelectStat(bool delaiLong = false)
        {
            return StatCache.Get("observation:stat", () => SqlDataAccess.SelectData<SelectStatObservationRequest>(SqlObservations.SelectStat, commandTimeout: delaiLong ? StatCache.TimeoutStatistiques : null));
        }

        public static bool Insert(AddObservationRequest AddObservationValues, Identity user)
        {
            int nbreRow = SqlDataAccess.SaveData(SqlObservations.Insert, AddObservationValues, user);
            return nbreRow > 0;
        }

        public static bool Update(ModifyObservationRequest modifyObservationValues, Identity user)
        {
            int nbreRow = SqlDataAccess.SaveData(SqlObservations.Update, modifyObservationValues, user);
            return nbreRow > 0;
        }

        public static bool Delete(int Id, Identity user)
        {
            int nbreRow = SqlDataAccess.SaveData(SqlObservations.Delete, new { Id }, user);
            return nbreRow > 0;
        }
    }
}