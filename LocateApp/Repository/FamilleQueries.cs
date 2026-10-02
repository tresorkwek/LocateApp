using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlFamille
    {
        public static string SelectAll { get; } = @"SELECT Id,Nom,IsActive,Couleur
                                                    FROM CategorieFamille";
        public static string SelectAllActive { get; } = @"SELECT Id,Nom,IsActive,Couleur
                                                        FROM CategorieFamille
                                                        WHERE IsActive = 1";
        public static string SelectAllStatPie { get; } = @"SELECT CategorieFamille.Nom AS label,Couleur AS color, Couleur AS highlight,
	                                                        (SELECT count(*) 
                                                             FROM Immo INNER JOIN Article ON Immo.IdArticle = Article.Id
			                                                           INNER JOIN Categorie ON Article.IdCategorie = Categorie.Id
		                                                               INNER JOIN CategorieFamille f ON Categorie.IdFamille = f.Id
	                                                         WHERE f.Id = CategorieFamille.Id AND Immo.LastEtat IS NOT NULL) AS value
                                                        FROM  CategorieFamille 
                                                        WHERE (SELECT count(*) 
                                                             FROM Immo INNER JOIN Article ON Immo.IdArticle = Article.Id
			                                                           INNER JOIN Categorie ON Article.IdCategorie = Categorie.Id
		                                                               INNER JOIN CategorieFamille f ON Categorie.IdFamille = f.Id
	                                                         WHERE f.Id = CategorieFamille.Id AND Immo.LastEtat IS NOT NULL) > 0";

		public static string SelectAllIdentifie { get; } = @"SELECT CategorieFamille.Id,CategorieFamille.Nom,CategorieFamille.IsActive, CategorieFamille.Couleur,
	                                                        (SELECT count(*) 
                                                             FROM Immo INNER JOIN Article ON Immo.IdArticle = Article.Id
			                                                           INNER JOIN Categorie ON Article.IdCategorie = Categorie.Id
		                                                               INNER JOIN CategorieFamille f ON Categorie.IdFamille = f.Id
	                                                         WHERE f.Id = CategorieFamille.Id AND Immo.QrCode IS NOT NULL) AS Nbre
                                                        FROM  CategorieFamille 
                                                        WHERE (SELECT count(*) 
                                                             FROM Immo INNER JOIN Article ON Immo.IdArticle = Article.Id
			                                                           INNER JOIN Categorie ON Article.IdCategorie = Categorie.Id
		                                                               INNER JOIN CategorieFamille f ON Categorie.IdFamille = f.Id
	                                                         WHERE f.Id = CategorieFamille.Id AND Immo.QrCode IS NOT NULL) > 0";
		public static string SelectAllStatPieLegend { get; } = @"SELECT Famille.Nom as label,Famille.Couleur AS color,convert(decimal(18),(convert(decimal(18,2),count(*)))/convert(decimal(18,2),Total) * 100) as data
                                                                FROM Immo INNER JOIN Article ON Immo.IdArticle = Article.Id
			                                                                    INNER JOIN Categorie ON Article.IdCategorie = Categorie.Id
		                                                                        INNER JOIN CategorieFamille Famille ON Categorie.IdFamille = Famille.Id,
			                                                                    (SELECT isnull(count(*),0) Total FROM Immo 
				                                                                INNER JOIN Article ON Immo.IdArticle = Article.Id
				                                                                INNER JOIN Categorie ON Article.IdCategorie = Categorie.Id
				                                                                INNER JOIN CategorieFamille f ON Categorie.IdFamille = f.Id
			                                                                    WHERE Immo.LastEtat IS NOT NULL) NbreImmo
                                                                WHERE Immo.LastEtat IS NOT NULL 
                                                                GROUP BY Famille.Nom,Famille.Couleur, NbreImmo.Total
                                                                HAVING convert(decimal(18),(convert(decimal(18,2),count(*)))/convert(decimal(18,2),Total) * 100) > 0";

        public static string SelectAllStatRadar { get; } = @"SELECT Famille.Nom as Label,
																   CONVERT(decimal(18),
	     
  																		  CONVERT(decimal(18,2),(SELECT count(*) Total 
			        																			 FROM Immo INNER JOIN Article ON Immo.IdArticle = Article.Id
									  																	   INNER JOIN Categorie ON Article.IdCategorie = Categorie.Id
																										   INNER JOIN CategorieFamille f ON Categorie.IdFamille = f.Id
																								 WHERE Immo.LastEtat = 'B' AND f.Id = Famille.Id AND Immo.IsActive = 1))/
																		 CONVERT(decimal(18,2), (SELECT count(*) Total 
																								 FROM Immo INNER JOIN Article ON Immo.IdArticle = Article.Id
										   																   INNER JOIN Categorie ON Article.IdCategorie = Categorie.Id
																										   INNER JOIN CategorieFamille f ON Categorie.IdFamille = f.Id
																								 WHERE Immo.LastEtat IS NOT NULL AND f.Id = Famille.Id  AND Immo.IsActive = 1)) *100
																	 ) AS PourcentageBon,

																	CONVERT(decimal(18),
	     
																			CONVERT(decimal(18,2),(SELECT count(*) Total 
																								   FROM Immo INNER JOIN Article ON Immo.IdArticle = Article.Id
																											 INNER JOIN Categorie ON Article.IdCategorie = Categorie.Id
																											 INNER JOIN CategorieFamille f ON Categorie.IdFamille = f.Id
																								   WHERE Immo.LastEtat = 'M' AND f.Id = Famille.Id AND Immo.IsActive = 1))/
																			CONVERT(decimal(18,2), (SELECT count(*) Total 
																									FROM Immo INNER JOIN Article ON Immo.IdArticle = Article.Id
																											  INNER JOIN Categorie ON Article.IdCategorie = Categorie.Id
																											  INNER JOIN CategorieFamille f ON Categorie.IdFamille = f.Id
																									WHERE Immo.LastEtat IS NOT NULL AND f.Id = Famille.Id AND Immo.IsActive = 1)) *100
																	  ) AS PourcentageMauvais
	
															FROM CategorieFamille Famille
															WHERE (SELECT count(*) Total 
																   FROM Immo INNER JOIN Article ON Immo.IdArticle = Article.Id
																			 INNER JOIN Categorie ON Article.IdCategorie = Categorie.Id
																			 INNER JOIN CategorieFamille f ON Categorie.IdFamille = f.Id
																   WHERE Immo.LastEtat IS NOT NULL AND f.Id = Famille.Id AND Immo.IsActive = 1) > 0";
        public static string SelectActiveByName { get; } = @"SELECT Id,Nom,IsActive,Couleur
                                                             FROM CategorieFamille
                                                             WHERE IsActive = 1 AND Nom LIKE @Nom ";
        public static string SelectById { get; } = @"SELECT Id,Nom,IsActive,Couleur
                                                    FROM CategorieFamille
                                                    WHERE Id = @Id";
        public static string SelectByName { get; } = @"SELECT Id,Nom,IsActive,Couleur
                                                       FROM CategorieFamille
                                                       WHERE Nom LIKE @Nom ";
        public static string SelectWithNumber { get; } = @"SELECT CategorieFamille.Id,CategorieFamille.Nom,CategorieFamille.IsActive,Couleur,
	                                                            (SELECT count(*) 
                                                                 FROM Immo INNER JOIN Article ON Immo.IdArticle = Article.Id
			                                                               INNER JOIN Categorie ON Article.IdCategorie = Categorie.Id
		                                                                   INNER JOIN CategorieFamille f ON Categorie.IdFamille = f.Id
	                                                             WHERE f.Id = CategorieFamille.Id AND Immo.LastEtat IS NOT NULL) AS Nbre
                                                            FROM  CategorieFamille 
                                                            WHERE (SELECT count(*) 
                                                                 FROM Immo INNER JOIN Article ON Immo.IdArticle = Article.Id
			                                                               INNER JOIN Categorie ON Article.IdCategorie = Categorie.Id
		                                                                   INNER JOIN CategorieFamille f ON Categorie.IdFamille = f.Id
	                                                             WHERE f.Id = CategorieFamille.Id AND Immo.LastEtat IS NOT NULL) > 0";
        public static string SelectWithNumberByEtat { get; } = @"SELECT CategorieFamille.Id,CategorieFamille.Nom,CategorieFamille.IsActive,Couleur,
	                                                                (SELECT count(*) 
                                                                     FROM Immo INNER JOIN Article ON Immo.IdArticle = Article.Id
			                                                                   INNER JOIN Categorie ON Article.IdCategorie = Categorie.Id
		                                                                       INNER JOIN CategorieFamille f ON Categorie.IdFamille = f.Id
	                                                                 WHERE f.Id = CategorieFamille.Id AND Immo.LastEtat = @Etat) AS Nbre
                                                                FROM  CategorieFamille 
                                                                WHERE (SELECT count(*) 
                                                                     FROM Immo INNER JOIN Article ON Immo.IdArticle = Article.Id
			                                                                   INNER JOIN Categorie ON Article.IdCategorie = Categorie.Id
		                                                                       INNER JOIN CategorieFamille f ON Categorie.IdFamille = f.Id
	                                                                 WHERE f.Id = CategorieFamille.Id AND Immo.LastEtat IS NOT NULL) > 0";
        public static string Insert { get; } = @"INSERT INTO CategorieFamille (Nom,Couleur,UserCreation)
                                                 VALUES (@Nom,@Couleur,@UserCreation)";
        public static string Update { get; } = @"UPDATE CategorieFamille 
                                                 SET Nom = @Nom, IsActive = @IsActive, DateCreation = @DateCreation, UserCreation = @UserCreation, Couleur = @Couleur
                                                 WHERE Id = @Id";
        public static string Delete { get; } = @"DELETE FROM CategorieFamille                                                  
                                                 WHERE Id = @Id";
    }
}