using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlOrganeType
    {
        public static string SelectAll { get; } = @"SELECT Id,Nom,Sigle,Ordre
                                                    FROM OrganeType
                                                    ORDER BY Ordre";
        public static string SelectById { get; } = @"SELECT Id,Nom,Sigle,Ordre
                                                     FROM OrganeType
                                                     WHERE Id = @Id";
        public static string Insert { get; } = @"INSERT INTO OrganeType(Nom,Sigle,Ordre)
                                                 VALUES (@Nom,@Sigle,@Ordre)";
        public static string Update { get; } = @"UPDATE OrganeType
                                                 SET Nom = @Nom, Sigle = @Sigle, Ordre = @Ordre
                                                 WHERE Id = @Id";
        public static string Delete { get; } = @"DELETE FROM OrganeType
                                                 WHERE Id = @Id";
    }
}