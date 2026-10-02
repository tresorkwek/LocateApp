using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlArticle
    {
        public static string SelectAll { get; } = @"SELECT Id,Code,Designation,IdPosteBudget,UniteMesure,CoutUnitaire,IdCategorie,Marque,Modele,IdFournisseur,IsActive,DateCreation,UserCreation
                                                    FROM Article";
        public static string SelectAllActive { get; } = @"SELECT Id,Code,Designation,IdPosteBudget,UniteMesure,CoutUnitaire,IdCategorie,Marque,Modele,IdFournisseur,IsActive,DateCreation,UserCreation
                                                          FROM Article
                                                          WHERE IsActive = 1";
        public static string SelectById { get; } = @"SELECT Id,Code,Designation,IdPosteBudget,UniteMesure,CoutUnitaire,IdCategorie,Marque,Modele,IdFournisseur,IsActive,DateCreation,UserCreation
                                                     FROM Article
                                                     WHERE Id = @Id";
        public static string SelectByLocal { get; } = @"SELECT Article.Id,Code,Designation,IdPosteBudget,UniteMesure,CoutUnitaire,IdCategorie,Marque,Modele,IdFournisseur,Article.IsActive,Article.DateCreation,Article.UserCreation, COUNT(*) AS NbreImmo
                                                        FROM Article INNER JOIN Immo ON Article.Id = Immo.IdArticle
                                                        WHERE Immo.IdLocal = @IdLocal AND Immo.IsActive = 1
                                                        GROUP BY Article.Id,Code,Designation,IdPosteBudget,UniteMesure,CoutUnitaire,IdCategorie,Marque,Modele,IdFournisseur,Article.IsActive,Article.DateCreation,Article.UserCreation";

        public static string SelectByOrgane { get; } = @"SELECT Article.Id,Article.Code,Article.Designation,IdPosteBudget,UniteMesure,CoutUnitaire,IdCategorie,Marque,Modele,IdFournisseur,Article.IsActive,Article.DateCreation,Article.UserCreation, COUNT(*) AS NbreImmo
                                                         FROM Article INNER JOIN Immo ON Article.Id = Immo.IdArticle
		                                                              INNER JOIN Local ON Immo.IdLocal = Local.Id
			                                                          INNER JOIN Organe ON Local.CodeOrgane = Organe.Id
                                                         WHERE Immo.IsActive = 1 AND Organe.Id = @CodeOrgane
                                                         GROUP BY Article.Id,Article.Code,Article.Designation,IdPosteBudget,UniteMesure,CoutUnitaire,IdCategorie,Marque,Modele,IdFournisseur,Article.IsActive,Article.DateCreation,Article.UserCreation";

        public static string SelectByEntite { get; } = @"SELECT Article.Id,Article.Code,Article.Designation,IdPosteBudget,UniteMesure,CoutUnitaire,IdCategorie,Marque,Modele,IdFournisseur,Article.IsActive,Article.DateCreation,Article.UserCreation, COUNT(*) AS NbreImmo
                                                         FROM Article INNER JOIN Immo ON Article.Id = Immo.IdArticle
		                                                              INNER JOIN Local ON Immo.IdLocal = Local.Id
			                                                          INNER JOIN Organe ON Local.CodeOrgane = Organe.Id
                                                         WHERE Immo.IsActive = 1 AND Organe.IdStructure = @CodeOrgane
                                                         GROUP BY Article.Id,Article.Code,Article.Designation,IdPosteBudget,UniteMesure,CoutUnitaire,IdCategorie,Marque,Modele,IdFournisseur,Article.IsActive,Article.DateCreation,Article.UserCreation";

        public static string SelectNameById { get; } = @"SELECT CONCAT(Marque,' ',Designation,' ',Modele) AS Designation
                                                     FROM Article
                                                     WHERE Id = @Id";
        public static string SelectByName { get; } = @"SELECT Id,Code,Designation,IdPosteBudget,UniteMesure,CoutUnitaire,IdCategorie,Marque,Modele,IdFournisseur,IsActive,DateCreation,UserCreation
                                                       FROM Article
                                                       WHERE Code LIKE @Code OR Designation LIKE @Designation";
        public static string SelectActiveByName { get; } = @"SELECT Id,Code,Designation,IdPosteBudget,UniteMesure,CoutUnitaire,IdCategorie,Marque,Modele,IdFournisseur,IsActive,DateCreation,UserCreation
                                                             FROM Article
                                                             WHERE IsActive = 1 AND (Code LIKE @Code OR Designation LIKE @Designation)";

        public static string Insert { get; } = @"spArticle_Insert";



        public static string Update { get; } = @"UPDATE Article 
                                                 SET Code = @Code,Designation = @Designation,IdPosteBudget = @IdPosteBudget,UniteMesure = @UniteMesure,CoutUnitaire = @CoutUnitaire,
                                                     IdCategorie = @IdCategorie,Marque = @Marque,Modele = @Modele,IdFournisseur = @IdFournisseur,IsActive = @IsActive,
                                                     UserCreation = @UserCreation, DateCreation = getdate()
                                                 WHERE Id = @Id";

        public static string Delete { get; } = @"DELETE FROM Article WHERE Id = @Id";

    }
}