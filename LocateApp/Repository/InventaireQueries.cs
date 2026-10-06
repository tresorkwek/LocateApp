using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlInventaire
    {
        public static string SelectAllEntete { get; } = @"SELECT Annee,DateCreation,UserCreation,DateCloture,UserCloture,Actif
                                                         FROM InventaireEntete
                                                         ORDER BY Annee DESC";

        public static string SelectAllEnteteCloturer { get; } = @"SELECT Annee,DateCreation,UserCreation,DateCloture,UserCloture,Actif
                                                                  FROM InventaireEntete
                                                                  WHERE UserCloture IS NOT NULL
                                                                  ORDER BY Annee DESC";

        public static string SelectDetails { get; } = @"SELECT Id,Annee,IdImmo,ImmoExist,Etat,IdObservation,DateCreation,UserCreation,Responsable,IdLocal,CodeOrgane,Observation,DesignationLocal
                                                        FROM InventaireDetails
                                                        WHERE Annee = @Annee";
        // Bilan d'une campagne par organe (page /inventaire/details/{annee})
        public static string SelectDetailsParOrgane { get; } = @"SELECT d.CodeOrgane, o.Nom AS NomOrgane, o.IdStructure, s.Nom AS NomStructure,
                                                                       COUNT(*) AS NbreLignes, COUNT(DISTINCT d.IdLocal) AS NbreLocaux,
                                                                       SUM(CASE WHEN d.ImmoExist = 1 THEN 1 ELSE 0 END) AS NbreVus,
                                                                       SUM(CASE WHEN d.ImmoExist = 1 THEN 0 ELSE 1 END) AS NbreNonVus,
                                                                       SUM(CASE WHEN d.ImmoExist = 1 AND d.Etat = 'B' THEN 1 ELSE 0 END) AS NbreBons,
                                                                       SUM(CASE WHEN d.ImmoExist = 1 AND d.Etat = 'M' THEN 1 ELSE 0 END) AS NbreMauvais,
                                                                       COUNT(DISTINCT d.UserCreation) AS NbreInventorieurs,
                                                                       MIN(d.DateCreation) AS PremierPassage, MAX(d.DateCreation) AS DernierPassage
                                                                FROM InventaireDetails d
                                                                     LEFT JOIN Organe o ON o.Id = d.CodeOrgane
                                                                     LEFT JOIN Organe s ON s.Id = o.IdStructure
                                                                WHERE d.Annee = @Annee
                                                                GROUP BY d.CodeOrgane, o.Nom, o.IdStructure, s.Nom
                                                                ORDER BY d.CodeOrgane";
        // Lignes d'une campagne pour un organe, avec les libellés utiles à l'affichage
        public static string SelectDetailsByOrgane { get; } = @"SELECT d.Id, d.Annee, d.IdImmo, d.ImmoExist, d.Etat, d.IdObservation, d.DateCreation, d.UserCreation, d.Responsable,
                                                                       d.IdLocal, d.CodeOrgane, d.Observation, ISNULL(NULLIF(d.DesignationLocal, ''), l.Designation) AS DesignationLocal,
                                                                       i.CodeImmo, a.Designation AS DesignationArticle, a.Marque, ob.Observation AS LibelleObservation, l.Code AS CodeLocal,
                                                                       LTRIM(RTRIM(ISNULL(u.Prenom, '') + ' ' + ISNULL(u.Nom, ''))) AS NomInventorieur,
                                                                       LTRIM(RTRIM(ISNULL(ag.Prenom, '') + ' ' + ISNULL(ag.Nom, ''))) AS NomResponsable
                                                                FROM InventaireDetails d
                                                                     LEFT JOIN Immo i ON i.Id = d.IdImmo
                                                                     LEFT JOIN Article a ON a.Id = i.IdArticle
                                                                     LEFT JOIN ImmoObservation ob ON ob.Id = d.IdObservation
                                                                     LEFT JOIN Local l ON l.Id = d.IdLocal
                                                                     LEFT JOIN _Utilisateur u ON u.UserName = d.UserCreation
                                                                     LEFT JOIN Agent ag ON ag.Matricule = d.Responsable + '00'
                                                                WHERE d.Annee = @Annee AND d.CodeOrgane = @CodeOrgane
                                                                ORDER BY ISNULL(NULLIF(d.DesignationLocal, ''), l.Designation), a.Designation";
        public static string SelectDetailsByImmo { get; } = @"SELECT Id,Annee,IdImmo,ImmoExist,Etat,IdObservation,DateCreation,UserCreation,Responsable,IdLocal,CodeOrgane,Observation,DesignationLocal
                                                              FROM InventaireDetails
                                                              WHERE Annee = @Annee AND IdImmo = @IdImmo";
        public static string SelectAnneeEnCours { get; } = @"SELECT Annee
                                                        FROM InventaireEntete
                                                        WHERE DateCloture IS NULL";
        public static string SelectLastAnneeComptable { get; } = @"SELECT TOP (1) Annee
                                                                   FROM InventaireEntete
                                                                   ORDER BY Annee DESC";
        public static string SelectEnteteById { get; } = @"SELECT Annee,DateCreation,UserCreation,DateCloture,UserCloture,Actif
                                                           FROM InventaireEntete
                                                           WHERE Annee = @Annee";

        public static string SelectEnteteCloturerById { get; } = @"SELECT Annee,DateCreation,UserCreation,DateCloture,UserCloture,Actif
                                                           FROM InventaireEntete
                                                           WHERE Annee = @Annee AND UserCloture IS NOT NULL";

        public static string SelectDetailsById { get; } = @"SELECT Id,Annee,IdImmo,ImmoExist,Etat,IdObservation,DateCreation,UserCreation,Responsable,IdLocal,CodeOrgane,Observation,DesignationLocal
                                                            FROM InventaireDetails
                                                            WHERE Id = @Id";
        // Inventorié = ligne InventaireDetails de l'année (écrite avec Immo.Inventorieur dans la même transaction) d'un bien identifié (UserVu).
        public static string SelectQuantiteImmoByArticle { get; } = @"SELECT count(*) as Nbre
                                                                      FROM InventaireDetails INNER JOIN Immo ON InventaireDetails.IdImmo = Immo.Id
                                                                      WHERE InventaireDetails.Annee = @Annee AND Immo.UserVu IS NOT NULL AND Immo.IdArticle = @IdArticle";
        public static string SelectQuantiteImmoByArticleAndLocal { get; } = @"SELECT count(*) as Nbre
                                                                      FROM InventaireDetails INNER JOIN Immo ON InventaireDetails.IdImmo = Immo.Id
                                                                      WHERE InventaireDetails.Annee = @Annee AND Immo.UserVu IS NOT NULL AND Immo.IdArticle = @IdArticle AND InventaireDetails.IdLocal = @IdLocal";

        public static string SelectQuantite { get; } = @"SELECT count(*) as Nbre
                                                         FROM InventaireDetails
                                                         WHERE ImmoExist = 1 AND Annee = @Annee";

        public static string SelectQuantiteByEtat { get; } = @"SELECT count(*) as Nbre
                                                                FROM InventaireDetails 
                                                                WHERE Annee = @Annee  AND Etat = @Etat";
        public static string SelectQuantiteNonVu { get; } = @"SELECT count(*) as Nbre
                                                                FROM InventaireDetails 
                                                                WHERE Annee = @Annee  AND ImmoExist = 0";

        public static string SelectQuantiteImmoByLocal { get; } = @"SELECT count(*) as Nbre
                                                                      FROM InventaireDetails INNER JOIN Immo ON InventaireDetails.IdImmo = Immo.Id
                                                                      WHERE InventaireDetails.Annee = @Annee AND Immo.UserVu IS NOT NULL AND InventaireDetails.IdLocal = @IdLocal";

        public static string SelectQuantiteImmoByOrgane { get; } = @"SELECT count(*) as Nbre
                                                                      FROM InventaireDetails INNER JOIN Immo ON InventaireDetails.IdImmo = Immo.Id
                                                                      WHERE InventaireDetails.Annee = @Annee AND Immo.UserVu IS NOT NULL AND InventaireDetails.CodeOrgane = @CodeOrgane";
        public static string SelectQuantiteImmoByResponsable { get; } = @"SELECT count(*) as Nbre
                                                                      FROM InventaireDetails INNER JOIN Immo ON InventaireDetails.IdImmo = Immo.Id
                                                                      WHERE InventaireDetails.Annee = @Annee AND Immo.UserVu IS NOT NULL AND InventaireDetails.Responsable = @Responsable";
        public static string InsertEntete { get; } = @"INSERT INTO InventaireEntete(Annee,UserCreation)
                                                       VALUES (@Annee,@UserCreation)";
        public static string InsertDetails { get; } = @"INSERT INTO InventaireDetails(Annee,IdImmo,ImmoExist,Etat,IdObservation,UserCreation,Responsable,IdLocal,CodeOrgane,DesignationLocal)
                                                        VALUES (@Annee,@IdImmo,@ImmoExist,@Etat,@IdObservation,@UserCreation,@Responsable,@IdLocal,@CodeOrgane,@DesignationLocal)";
        public static string InsertDetailsLocal { get; } = @"INSERT INTO InventaireDetails(Annee,IdImmo,ImmoExist,Etat,IdObservation,UserCreation,Responsable,IdLocal,CodeOrgane,DesignationLocal) 
                                                            SELECT @Annee AS Annee,Immo.Id AS IdImmo,ImmoExist,LastEtat AS Etat,IdLastObservation AS IdObservation,@UserCreation AS UserCreation,Responsable,IdLocal,@CodeOrgane AS CodeOrgane,Local.Designation
                                                            FROM Immo LEFT JOIN ImmoObservation ON Immo.IdLastObservation = ImmoObservation.Id
                                                                 INNER JOIN Local ON Immo.IdLocal = Local.Id
                                                            WHERE Immo.IdLocal = @IdLocal AND Immo.IsActive = 1 AND Immo.QrCode IS NOT NULL AND Immo.UserVu IS NOT NULL AND ImmoExist = 1 AND (Inventorieur IS NULL OR LastAnneeComptable != @Annee)";
        public static string InsertDetailsNonVu { get; } = @"INSERT INTO InventaireDetails(Annee,IdImmo,ImmoExist,Etat,IdObservation,UserCreation,Responsable,IdLocal,CodeOrgane,Observation,DesignationLocal)
                                                             VALUES (@Annee,@IdImmo,0,NULL,NULL,@UserCreation,@Responsable,@IdLocal,@CodeOrgane,@Observation,@DesignationLocal)";
        public static string UpdateEntete { get; } = @"UPDATE InventaireEntete
                                                       SET Actif = @Actif
                                                       WHERE Annee = @Annee";
        public static string Cloture { get; } = @"UPDATE InventaireEntete
                                                       SET UserCloture = @UserCloture,DateCloture = getdate(),Actif = 0
                                                       WHERE Annee = @Annee";
        public static string UpdateDetails { get; } = @"UPDATE InventaireDetails
                                                        SET Annee = @Annee,IdImmo = @IdImmo,ImmoExist = @ImmoExist,Etat = @Etat,IdObservation = @IdObservation,Responsable = @Responsable,IdLocal = @IdLocal,CodeOrgane = @CodeOrgane, Observation = @Observation
                                                        WHERE Id = @Id";
    }
}