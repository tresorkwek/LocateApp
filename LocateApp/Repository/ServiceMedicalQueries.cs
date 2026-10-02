using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlServiceMedical
    {
        public static string SelectAll { get; } = @"SELECT IdService, Denomination,Acronyme, AutoApprobation
                                                    FROM ServiceMedical";

        public static string SelectById { get; } = @"SELECT IdService, Denomination, Acronyme,AutoApprobation
                                                     FROM ServiceMedical
                                                     WHERE IdService = @IdService";
        public static string Insert { get; } = @"INSERT INTO ServiceMedical(IdService,Denomination,Acronyme,AutoApprobation)
                                                 VALUES (@IdService, @Denomination,@Acronyme,@AutoApprobation)";

        public static string Update { get; } = @"UPDATE ServiceMedical 
                                                 SET Denomination = @Denomination, Acronyme = @Acronyme, AutoApprobation = @AutoApprobation
                                                 WHERE IdService = @IdService";

        public static string Delete { get; } = @"DELETE FROM ServiceMedical 
                                                 WHERE IdService = @IdService";
                
    }
}