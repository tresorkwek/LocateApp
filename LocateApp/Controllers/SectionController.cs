using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Repository;

namespace LocateApp.Controllers
{
    public static class SectionController
    {
        public static List<Section> GetSection(int? idSection = null)
        {
            string sql = idSection == null ? SqlSection.SelectAll : SqlSection.SelectById;
            var sectionValue = idSection == null ? null : new { IdSection = idSection };

            return SqlDataAccess.SelectData<Section>(sql, sectionValue);
        }
    }
}