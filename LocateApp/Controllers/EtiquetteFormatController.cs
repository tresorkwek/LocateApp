using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.DataTransferObjects;
using LocateApp.Models;
using LocateApp.Repository;

namespace LocateApp.Controllers
{
    public static class EtiquetteFormatController
    {
        public static List<EtiquetteFormat> Select(int? Id = null,bool selectAll=false)
        {
            string sql = Id == null ? (selectAll ? SqlEtiquetteFormat.SelectAll : SqlEtiquetteFormat.SelectAllPublic) : SqlEtiquetteFormat.SelectById;
            return SqlDataAccess.SelectData<EtiquetteFormat>(sql, new { Id });
        }

        public static bool Insert(EtiquetteFormatInsertRequest AddEtiquetteFormatValues, Identity user)
        {
            int nbreRow = SqlDataAccess.SaveData(SqlEtiquetteFormat.Insert, AddEtiquetteFormatValues, user);
            return nbreRow > 0;
        }

        public static bool Update(EtiquetteFormatModifytRequest modifyEtiquetteFormatValues, Identity user)
        {
            int nbreRow = SqlDataAccess.SaveData(SqlEtiquetteFormat.Update, modifyEtiquetteFormatValues, user);
            return nbreRow > 0;
        }

        public static bool Delete(int Id, Identity user)
        {
            int nbreRow = SqlDataAccess.SaveData(SqlEtiquetteFormat.Delete, new { Id }, user);
            return nbreRow > 0;
        }

    }
}