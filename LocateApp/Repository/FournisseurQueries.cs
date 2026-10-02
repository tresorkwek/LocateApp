using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlFournisseur
    {
        public static string SelectAll { get; } = @"SELECT Id,Nom,Adresse,IsActive,DateCreation,UserCreation
                                                    FROM ArticleFournisseur";
        public static string SelectAllActive { get; } = @"SELECT Id,Nom,Adresse,IsActive,DateCreation,UserCreation
                                                          FROM ArticleFournisseur
                                                          WHERE IsActive = 1";
        public static string SelectById { get; } = @"SELECT Id,Nom,Adresse,IsActive,DateCreation,UserCreation
                                                     FROM ArticleFournisseur
                                                     WHERE Id = @Id";
        public static string SelectByName { get; } = @"SELECT Id,Nom,Adresse,IsActive,DateCreation,UserCreation
                                                       FROM ArticleFournisseur
                                                       WHERE Nom LIKE @Nom";
        public static string SelectActiveByName { get; } = @"SELECT Id,Nom,Adresse,IsActive,DateCreation,UserCreation
                                                             FROM ArticleFournisseur
                                                             WHERE IsActive = 1 AND Nom LIKE @Nom";
        public static string Insert { get; } = @"INSERT INTO ArticleFournisseur (Nom,Adresse,UserCreation)
                                                 VALUES (@Nom,@Adresse,@UserCreation)";

        public static string Update { get; } = @"UPDATE ArticleFournisseur 
                                                 SET Nom = @Nom, Adresse = @Adresse, IsActive = @IsActive, DateCreation = @DateCreation, UserCreation = @UserCreation
                                                 WHERE Id = @Id";
        public static string Delete { get; } = @"DELETE FROM ArticleFournisseur                                                  
                                                 WHERE Id = @Id";

    }
}