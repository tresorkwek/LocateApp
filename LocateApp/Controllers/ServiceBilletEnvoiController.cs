using LocateApp.Models;
using LocateApp.Repository;
using LocateApp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Controllers
{
    public static class ServiceBilletEnvoiController
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(ServiceBilletEnvoiController));

        public static List<ServiceBilletEnvoi> GetServiceHopitalByBilletEnvoi(long idBilletEnvoi)
        {
            string sql = SqlServiceBilletEnvoi.SelectAll;

            return SqlDataAccess.SelectData<ServiceBilletEnvoi>(sql, new { IdBilletEnvoi = idBilletEnvoi });
        }

        public static List<PassagePatient> GetPassagePatientByBilletEnvoiAndService(long IdBilletEnvoi, long IdServiceHopital)
        {
            return SqlDataAccess.SelectData<PassagePatient>(SqlServiceBilletEnvoi.SelectUsedByService, new { IdBilletEnvoi, IdServiceHopital });
        }        

    }
}