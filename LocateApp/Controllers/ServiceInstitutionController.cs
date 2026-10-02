using LocateApp.Models;
using LocateApp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Controllers
{
    public static class ServiceInstitutionController
    {
        public static List<ServiceInstitution> GetServiceHopitalByHopital(string idInstitution)
        {
            string sql = SqlServiceInstitution.SelectByInstitution;

            return SqlDataAccess.SelectData<ServiceInstitution>(sql, new { IdInstitution = idInstitution });
        }
        public static ServiceInstitution GetServiceHopitalByid(int idServiceHopital)
        {
            return SqlDataAccess.SelectData<ServiceInstitution>(SqlServiceInstitution.SelectById, new { IdServiceHopital = idServiceHopital }).FirstOrDefault();
        }

    }
}