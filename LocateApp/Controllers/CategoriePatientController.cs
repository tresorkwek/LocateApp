using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Repository;
using LocateApp.DataTransferObjects;

namespace LocateApp.Controllers
{
    public static class CategoriePatientController
    {
        public static List<CategorieAgent> SelectCategoriePatients(int id = 0)
        {
            string sql = (id == 0) ? SqlSelectCategoriePatient.All : SqlSelectCategoriePatient.Id;

            CategoriePatientRequest parameters = new CategoriePatientRequest { 
                id =id
            };

            return SqlDataAccess.SelectData<CategorieAgent>(sql,parameters);
        }
    }
}