using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlCategorie
    {
        public static string SelectAll { get; } = @"SELECT Categorie.Id,Designation,Categorie.IsActive,IdFamille,CategorieFamille.nom AS Famille,Categorie.DateCreation,Categorie.UserCreation
                                                    FROM Categorie LEFT JOIN CategorieFamille ON Categorie.IdFamille = CategorieFamille.Id";
        public static string SelectAllActive { get; } = @"SELECT Categorie.Id,Designation,Categorie.IsActive,IdFamille,CategorieFamille.nom AS Famille,Categorie.DateCreation,Categorie.UserCreation
                                                          FROM Categorie LEFT JOIN CategorieFamille ON Categorie.IdFamille = CategorieFamille.Id
                                                          WHERE Categorie.IsActive = 1";
        public static string SelectById { get; } = @"SELECT Categorie.Id,Designation,Categorie.IsActive,IdFamille,CategorieFamille.nom AS Famille,Categorie.DateCreation,Categorie.UserCreation
                                                     FROM Categorie LEFT JOIN CategorieFamille ON Categorie.IdFamille = CategorieFamille.Id
                                                     WHERE Categorie.Id = @Id";
        public static string SelectByName { get; } = @"SELECT Categorie.Id,Designation,Categorie.IsActive,IdFamille,CategorieFamille.nom AS Famille,Categorie.DateCreation,Categorie.UserCreation
                                                       FROM Categorie LEFT JOIN CategorieFamille ON Categorie.IdFamille = CategorieFamille.Id
                                                       WHERE Designation LIKE @Designation";
        public static string SelectActiveByName { get; } = @"SELECT Categorie.Id,Designation,Categorie.IsActive,IdFamille,CategorieFamille.nom AS Famille,Categorie.DateCreation,Categorie.UserCreation
                                                             FROM Categorie LEFT JOIN CategorieFamille ON Categorie.IdFamille = CategorieFamille.Id
                                                             WHERE Categorie.IsActive = 1 AND Designation LIKE @Designation";

        public static string Insert { get; } = @"INSERT INTO Categorie (Designation,IdFamille,UserCreation)
                                                 VALUES (@Designation,@IdFamille,@UserCreation)";

        public static string Update { get; } = @"UPDATE Categorie 
                                                 SET Designation = @Designation, IsActive = @IsActive, IdFamille = @IdFamille, DateCreation = @DateCreation, UserCreation = @UserCreation
                                                 WHERE Id = @Id";
        public static string Delete { get; } = @"DELETE FROM Categorie                                                  
                                                 WHERE Id = @Id";

    }
}