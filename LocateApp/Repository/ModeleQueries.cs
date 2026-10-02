using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlModele
    {
        public static string SelectAll { get; } = @"SELECT Id,Designation,IdMarque,IsActive
                                                    FROM Modele";
        public static string SelectAllActive { get; } = @"SELECT Id,Designation,IdMarque,IsActive
                                                          FROM Modele
                                                          WHERE IsActive = 1";
        public static string SelectById { get; } = @"SELECT Id,Designation,IdMarque,IsActive
                                                     FROM Modele
                                                     WHERE Id = @Id";
        public static string SelectByName { get; } = @"SELECT Id,Designation,IdMarque,IsActive
                                                       FROM Modele
                                                       WHERE Designation LIKE @Designation";
        public static string SelectActiveByName { get; } = @"SELECT Id,Designation,IdMarque,IsActive
                                                             FROM Modele
                                                             WHERE IsActive = 1 AND Designation LIKE @Designation";
      
    }
}