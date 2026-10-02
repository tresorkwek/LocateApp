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