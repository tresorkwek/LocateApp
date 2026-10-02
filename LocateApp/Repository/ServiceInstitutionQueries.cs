using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlServiceInstitution
    {
        public static string SelectByInstitution { get; } = @"SELECT SerialId IdService, CASE WHEN sh.Denomination IS NULL THEN sm.Denomination ELSE sh.Denomination END Denomination,sm.IdService AS IdServiceMedical 
                                                              FROM ServiceMedical sm INNER JOIN ServicesHopital sh ON sm.IdService = sh.RefServiceMed
                                                              WHERE sh.IsActive = 1 AND sh.RefHopitalConv = @IdInstitution";
        public static string SelectById { get; } = @"SELECT SerialId IdService, CASE WHEN sh.Denomination IS NULL THEN sm.Denomination ELSE sh.Denomination END Denomination,sm.IdService AS IdServiceMedical 
                                                     FROM ServiceMedical sm INNER JOIN ServicesHopital sh ON sm.IdService = sh.RefServiceMed
                                                     WHERE SerialId = @IdServiceHopital";

        public static string desactiver { get; } = @"UPDATE ServicesHopital
                                                     SET IsActive = 0
                                                     WHERE  RefHopitalConv = @Id AND RefServiceMed = @IdService";

        public static string insert { get; } = @"INSERT INTO ServicesHopital (RefHopitalConv,RefServiceMed,UserCreation)
                                                 VALUES (@Id,@IdService,@UserCreation)";

    }
}