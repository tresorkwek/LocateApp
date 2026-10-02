using LocateApp.DataTransferObjects;
using LocateApp.Models;
using LocateApp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Controllers
{
    public static class ServiceMedicalController
    {        
        public static List<ServiceMedical> GetService(string idService = null)
        {
            string sql = (idService == null) ? SqlServiceMedical.SelectAll : SqlServiceMedical.SelectById;
            var values = (idService == null) ? null : new { IdService = idService };

            return SqlDataAccess.SelectData<ServiceMedical>(sql, values);
        }   

        public static bool InsertServiceMedical(UpSetServiceMedicalRequest InsertServiceMedical,Identity user)
        {
            int nbreRow = SqlDataAccess.SaveData(SqlServiceMedical.Insert, InsertServiceMedical, user);

            return nbreRow > 0;
        }

        public static bool ModifyServiceMedical(UpSetServiceMedicalRequest UpdateServiceMedicalRequest, Identity user)
        {
            int nbreRow = SqlDataAccess.SaveData(SqlServiceMedical.Update, UpdateServiceMedicalRequest, user);

            return nbreRow > 0;
        }

        public static bool Delete(string idService,Identity user)
        {
            string sql = SqlServiceMedical.Delete;

            try
            {
                SqlDataAccess.SaveData(sql, new { IdService = idService } ,user);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}