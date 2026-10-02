using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlOrgane
    {
        public static string SelectAll { get; } = @"SELECT Id, Nom, Sigle,IndCoresp,Fictif,Actif,IdTypeOrgane,IdOrganeParent,Interne,Ordre,IdStructure,OrdreInterne,Adresse,Version,Entite
                                                    FROM Organe
                                                    WHERE Version = (SELECT TOP 1 Id FROM OrganeVersion WHERE Defaut = 1)
                                                    ORDER BY ordre, OrdreInterne,IdStructure,Id";
        public static string SelectAllByVersion { get; } = @"SELECT Id, Nom, Sigle,IndCoresp,Fictif,Actif,IdTypeOrgane,IdOrganeParent,Interne,Ordre,IdStructure,OrdreInterne,Adresse,Version,Entite
                                                             FROM Organe
                                                             WHERE Version = @Version 
                                                             ORDER BY ordre, OrdreInterne,IdStructure,Id";
        // Les comptages de biens sont calculés en une seule passe (agrégat par organe) au lieu de trois sous-requêtes par ligne.
        public static string SelectAllWithNumber { get; } = @"SELECT Organe.Id, Organe.Nom, Sigle,IndCoresp,Fictif,Actif,IdTypeOrgane,IdOrganeParent,Interne,Ordre,IdStructure,OrdreInterne,Adresse,Version,Entite,
                                                                       ISNULL(Stat.NbreBien, 0) AS NbreBien,
                                                                       ISNULL(Stat.NbreBienIdentifie, 0) AS NbreBienIdentifie,
                                                                       ISNULL(Stat.NbreBienInventorie, 0) AS NbreBienInventorie
                                                                FROM Organe
                                                                     LEFT JOIN (SELECT Local.CodeOrgane,
                                                                                       SUM(CASE WHEN Local.IdTypeLocal = 1 THEN 1 ELSE 0 END) AS NbreBien,
                                                                                       SUM(CASE WHEN Immo.UserVu IS NOT NULL THEN 1 ELSE 0 END) AS NbreBienIdentifie,
                                                                                       SUM(CASE WHEN Immo.UserVu IS NOT NULL AND Immo.Inventorieur IS NOT NULL AND Immo.LastAnneeComptable = Ent.AnneeEnCours THEN 1 ELSE 0 END) AS NbreBienInventorie
                                                                                FROM Immo INNER JOIN Local ON Immo.IdLocal = Local.Id LEFT JOIN (SELECT TOP 1 Annee AS AnneeEnCours FROM InventaireEntete WHERE DateCloture IS NULL) Ent ON 1 = 1
                                                                                WHERE Immo.ImmoExist = 1 AND Immo.IsActive = 1
                                                                                GROUP BY Local.CodeOrgane) Stat ON Stat.CodeOrgane = Organe.Id
                                                                WHERE Actif = 1 AND Version = (SELECT TOP 1 Id FROM OrganeVersion WHERE Defaut = 1)
                                                                ORDER BY ordre, OrdreInterne,IdStructure,Id";
        public static string SelectByName { get; } = @"SELECT Id, Nom, Sigle,IndCoresp,Fictif,Actif,IdTypeOrgane,IdOrganeParent,Interne,Ordre,IdStructure,OrdreInterne,Adresse,Version,Entite
                                                     FROM Organe
                                                     WHERE  (Id LIKE @ValueToSelect OR Nom LIKE @ValueToSelect OR Sigle LIKE @ValueToSelect) AND Version = (SELECT TOP 1 Id FROM OrganeVersion WHERE Defaut = 1)
                                                     ORDER BY ordre, OrdreInterne,IdStructure,Id";
        public static string SelectById { get; } = @"SELECT Id, Nom, Sigle,IndCoresp,Fictif,Actif,IdTypeOrgane,IdOrganeParent,Interne,Ordre,IdStructure,OrdreInterne,Adresse,Version,Entite
                                                     FROM Organe
                                                     WHERE Id = @Id ";
        public static string SelectNameById { get; } = @"SELECT Nom
                                                     FROM Organe
                                                     WHERE Id = @Id ";
        public static string SelectName { get; } = @"SELECT Nom
                                                         FROM Organe
                                                         WHERE  (Id LIKE @ValueToSelect OR Nom LIKE @ValueToSelect OR Sigle LIKE @ValueToSelect) AND Version = (SELECT TOP 1 Id FROM OrganeVersion WHERE Defaut = 1)
                                                         ORDER BY ordre, OrdreInterne,IdStructure,Id";
        public static string SelectOrganesStructureById { get; } = @"SELECT Id, Nom, Sigle,IndCoresp,Fictif,Actif,IdTypeOrgane,IdOrganeParent,Interne,Ordre,IdStructure,OrdreInterne,Adresse,Version,Entite
                                                             FROM Organe
                                                             WHERE IdStructure = @Id
                                                             ORDER BY ordre, OrdreInterne,IdStructure,Id";
        public static string SelectOrganesStructureByIdWithNumber { get; } = @"SELECT Organe.Id, Organe.Nom, Sigle,IndCoresp,Fictif,Actif,IdTypeOrgane,IdOrganeParent,Interne,Ordre,IdStructure,OrdreInterne,Adresse,Version,Entite,
                                                                                       ISNULL(Stat.NbreBien, 0) AS NbreBien,
                                                                                       ISNULL(Stat.NbreBienIdentifie, 0) AS NbreBienIdentifie,
                                                                                       ISNULL(Stat.NbreBienInventorie, 0) AS NbreBienInventorie
                                                                                FROM Organe
                                                                                     LEFT JOIN (SELECT Local.CodeOrgane,
                                                                                                       SUM(CASE WHEN Local.IdTypeLocal = 1 THEN 1 ELSE 0 END) AS NbreBien,
                                                                                                       SUM(CASE WHEN Immo.UserVu IS NOT NULL THEN 1 ELSE 0 END) AS NbreBienIdentifie,
                                                                                                       SUM(CASE WHEN Immo.UserVu IS NOT NULL AND Immo.Inventorieur IS NOT NULL AND Immo.LastAnneeComptable = Ent.AnneeEnCours THEN 1 ELSE 0 END) AS NbreBienInventorie
                                                                                                FROM Immo INNER JOIN Local ON Immo.IdLocal = Local.Id LEFT JOIN (SELECT TOP 1 Annee AS AnneeEnCours FROM InventaireEntete WHERE DateCloture IS NULL) Ent ON 1 = 1
                                                                                                WHERE Immo.ImmoExist = 1 AND Immo.IsActive = 1
                                                                                                GROUP BY Local.CodeOrgane) Stat ON Stat.CodeOrgane = Organe.Id
                                                                                WHERE Actif = 1 AND IdStructure = @Id
                                                                                ORDER BY ordre, OrdreInterne,IdStructure,Id";
        public static string SelectEntite { get; } = @"SELECT Id, Nom, Sigle,IndCoresp,Fictif,Actif,IdTypeOrgane,IdOrganeParent,Interne,Ordre,IdStructure,OrdreInterne,Adresse,Version,Entite
                                                       FROM Organe
                                                       WHERE Actif = 1 AND Id = IdStructure AND Version = (SELECT TOP 1 Id FROM OrganeVersion WHERE Defaut = 1)
                                                       ORDER BY ordre, OrdreInterne,IdStructure,Id";

        // Comptage des biens par structure en une seule passe (agrégat sur O.IdStructure).
        public static string SelectEntiteWithNumber { get; } = @"SELECT Organe.Id, Organe.Nom, Organe.Sigle, Organe.IndCoresp, Organe.Fictif, Organe.Actif, Organe.IdTypeOrgane, Organe.IdOrganeParent, Organe.Interne, Organe.Ordre, Organe.IdStructure, Organe.OrdreInterne, Organe.Adresse, Organe.Version, Organe.Entite,
                                                                       ISNULL(Stat.NbreBien, 0) AS NbreBien
                                                                FROM Organe
                                                                     LEFT JOIN (SELECT O.IdStructure, COUNT(*) AS NbreBien
                                                                                FROM Immo INNER JOIN Local ON Immo.IdLocal = Local.Id
                                                                                          INNER JOIN Organe O ON Local.CodeOrgane = O.Id
                                                                                WHERE Immo.ImmoExist = 1 AND Immo.IsActive = 1 AND Local.IdTypeLocal = 1
                                                                                GROUP BY O.IdStructure) Stat ON Stat.IdStructure = Organe.Id
                                                                WHERE Organe.Actif = 1 AND Organe.Id = Organe.IdStructure AND Organe.Version = (SELECT TOP 1 Id FROM OrganeVersion WHERE Defaut = 1)
                                                                ORDER BY Organe.Ordre, Organe.OrdreInterne, Organe.IdStructure, Organe.Id";
        public static string SelectEntiteWithDetails { get; } = @"SELECT Organe.Id, Organe.Nom, Sigle,IndCoresp,Fictif,Actif,IdTypeOrgane,IdOrganeParent,Interne,Ordre,IdStructure,OrdreInterne,Adresse,Version,Entite,
                                                                        (SELECT COUNT(*) Nbre
	                                                                    FROM InventaireDetails INNER JOIN Organe O ON InventaireDetails.CodeOrgane = O.Id
	                                                                    WHERE O.IdStructure = Organe.Id AND InventaireDetails.ImmoExist = 1 AND InventaireDetails.Annee = @Annee)  As NbreBienInventorie,	
		
	                                                                    (SELECT COUNT(*) Nbre
	                                                                    FROM Immo INNER JOIN Local ON Immo.IdLocal = Local.Id
	                                                                                INNER JOIN Organe O ON Local.CodeOrgane = O.Id
	                                                                    WHERE O.IdStructure = Organe.Id AND Immo.UserVu IS NOT NULL AND Immo.ImmoExist = 1 AND Immo.IsActive = 1  )  As NbreBienIdentifie,	
                                                                        
                                                                        (SELECT COUNT(*) Nbre
	                                                                    FROM Immo INNER JOIN Local ON Immo.IdLocal = Local.Id   
	                                                                                INNER JOIN Organe O ON Local.CodeOrgane = O.Id
	                                                                    WHERE O.IdStructure = Organe.Id AND Immo.ImmoExist = 1 AND Immo.IsActive = 1 AND Local.IdTypeLocal =2 )  As NbreBienNonVu,
		
	                                                                    (SELECT COUNT(*) Nbre
	                                                                    FROM Immo INNER JOIN Local ON Immo.IdLocal = Local.Id   
	                                                                                INNER JOIN Organe O ON Local.CodeOrgane = O.Id
	                                                                    WHERE O.IdStructure = Organe.Id AND Immo.ImmoExist = 1 AND Immo.IsActive = 1 AND Local.IdTypeLocal = 1)  As NbreBien
                                                                FROM Organe
                                                                WHERE Actif = 1 AND Organe.Id = IdStructure AND Version = (SELECT TOP 1 Id FROM OrganeVersion WHERE Defaut = 1)
                                                                ORDER BY ordre, OrdreInterne,IdStructure,Id";

        public static string SelectEntiteWithNumberById { get; } = @"SELECT Organe.Id, Organe.Nom, Organe.Sigle, Organe.IndCoresp, Organe.Fictif, Organe.Actif, Organe.IdTypeOrgane, Organe.IdOrganeParent, Organe.Interne, Organe.Ordre, Organe.IdStructure, Organe.OrdreInterne, Organe.Adresse, Organe.Version, Organe.Entite,
                                                                       ISNULL(Stat.NbreBien, 0) AS NbreBien
                                                                FROM Organe
                                                                     LEFT JOIN (SELECT O.IdStructure, COUNT(*) AS NbreBien
                                                                                FROM Immo INNER JOIN Local ON Immo.IdLocal = Local.Id
                                                                                          INNER JOIN Organe O ON Local.CodeOrgane = O.Id
                                                                                WHERE Immo.ImmoExist = 1 AND Immo.IsActive = 1 AND Local.IdTypeLocal = 1
                                                                                GROUP BY O.IdStructure) Stat ON Stat.IdStructure = Organe.Id
                                                                WHERE Organe.Actif = 1 AND Organe.Id = Organe.IdStructure AND Organe.Id = @Id
                                                                ORDER BY Organe.Ordre, Organe.OrdreInterne, Organe.IdStructure, Organe.Id";

        public static string SelectEntiteWithDetailsById { get; } = @"SELECT Organe.Id, Organe.Nom, Sigle,IndCoresp,Fictif,Actif,IdTypeOrgane,IdOrganeParent,Interne,Ordre,IdStructure,OrdreInterne,Adresse,Version,Entite,
                                                                        (SELECT COUNT(*) Nbre
	                                                                    FROM InventaireDetails INNER JOIN Organe O ON InventaireDetails.CodeOrgane = O.Id
	                                                                    WHERE O.IdStructure = Organe.Id AND InventaireDetails.ImmoExist = 1 AND InventaireDetails.Annee = @Annee)  As NbreBienInventorie,	
		
	                                                                    (SELECT COUNT(*) Nbre
	                                                                    FROM Immo INNER JOIN Local ON Immo.IdLocal = Local.Id  
	                                                                                INNER JOIN Organe O ON Local.CodeOrgane = O.Id
	                                                                    WHERE O.IdStructure = Organe.Id AND Immo.QrCode IS NOT NULL AND Immo.ImmoExist = 1 AND Immo.IsActive = 1 )  As NbreBienIdentifie,	 
		
	                                                                    (SELECT COUNT(*) Nbre
	                                                                    FROM Immo INNER JOIN Local ON Immo.IdLocal = Local.Id   
	                                                                                INNER JOIN Organe O ON Local.CodeOrgane = O.Id
	                                                                    WHERE O.IdStructure = Organe.Id AND Immo.ImmoExist = 1 AND Immo.IsActive = 1 AND Local.IdTypeLocal = 1)  As NbreBien
                                                                FROM Organe
                                                                WHERE Actif = 1 AND Organe.Id = IdStructure AND Organe.Id = @Id
                                                                ORDER BY ordre, OrdreInterne,IdStructure,Id";

        public static string SelectTypeById { get; } = @"SELECT OrganeType.Nom
                                                         FROM Organe INNER JOIN OrganeType ON Organe.IdTypeOrgane = OrganeType.Id
                                                         WHERE Organe.Id = @Id ";

        public static string SelectNbreLocauxById { get; } = @"SELECT COUNT(*) Nbre
                                                               FROM Local
                                                               WHERE CodeOrgane = @Id ";
        public static string SelectMaxOrdreInterneByStructure { get; } = @"SELECT max(OrdreInterne) AS OrdreInterne
                                                                           FROM Organe
                                                                           WHERE IdStructure = @IdStructure AND Version = (SELECT TOP 1 Id FROM OrganeVersion WHERE Defaut = 1)";
        public static string Insert { get; } = @"INSERT INTO Organe(Id,Nom,Sigle,IndCoresp,Fictif,Actif,IdTypeOrgane,IdOrganeParent,Interne,Ordre,IdStructure,OrdreInterne,Adresse,Version,Entite)
                                                 VALUES(@Id,@Nom,@Sigle,@IndCoresp,@Fictif,@Actif,@IdTypeOrgane,@IdOrganeParent,@Interne,@Ordre,@IdStructure,@OrdreInterne,@Adresse,@Version,@Entite)";
        public static string Update { get; } = @"UPDATE Organe
                                                 SET Nom = @Nom, Sigle = @Sigle, IndCoresp = @IndCoresp, Fictif = @Fictif, Actif = @Actif, IdTypeOrgane = @IdTypeOrgane, IdOrganeParent = @IdOrganeParent, Interne = @Interne, Ordre = @Ordre, IdStructure = @IdStructure, OrdreInterne = @OrdreInterne, Adresse = @Adresse, Version = @Version, Entite = @Entite
                                                 WHERE Id = @Id";                                                  

    
        /* Glisser-déposer : nouveau parent, nouvel ordre parmi les frères, structure de rattachement recalculée.
           Les lignes arrivent dans l'ordre de l'arbre (parents avant enfants) : la structure se propage. */
        public static string UpdateParentOrdre { get; } = @"UPDATE o
                                                            SET IdOrganeParent = @Parent,
                                                                Ordre = CASE WHEN @Parent IS NULL THEN @Ordre ELSE ISNULL(p.Ordre, @Ordre) END,
                                                                OrdreInterne = @Ordre,
                                                                IdStructure = CASE WHEN o.Id = o.IdStructure OR o.Entite = 1 THEN o.Id ELSE ISNULL(p.IdStructure, o.Id) END
                                                            FROM Organe o LEFT JOIN Organe p ON p.Id = @Parent
                                                            WHERE o.Id = @Id";

        /* Ce qui empêche la suppression physique d'un organe */
        public static string SelectDependances { get; } = @"SELECT (SELECT COUNT(*) FROM Organe WHERE IdOrganeParent = @Id) AS Enfants,
                                                                   (SELECT COUNT(*) FROM Organe WHERE IdStructure = @Id AND Id <> @Id) AS Rattaches,
                                                                   (SELECT COUNT(*) FROM Local WHERE CodeOrgane = @Id) AS Locaux,
                                                                   (SELECT COUNT(*) FROM _Utilisateur WHERE CodeOrgane = @Id OR IdInstitution = @Id) AS Utilisateurs,
                                                                   (SELECT COUNT(*) FROM InventaireDetails WHERE CodeOrgane = @Id) AS Inventaires";

        public static string Delete { get; } = @"DELETE FROM Organe WHERE Id = @Id";

}
}