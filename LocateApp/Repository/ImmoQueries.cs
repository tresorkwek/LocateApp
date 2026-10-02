using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlImmo
    {
        public static string SelectAll { get; } = @"SELECT Immo.Id,CodeADM,CodeImmo,IdArticle,IdLocal,DateCreation,UserCreation,QrCode,IdEtiquette,IsActive,LastEtat,IdLastObservation,LastAnneeComptable,Responsable,Inventorieur,DateInventaire,ImmoExist,ImmoObservation.Observation as LastObservation,IdImmoParent,IdImmoPrincipal,Immo.Observation,DateMisEnService,UserMisEnService,DateDeclassement,UserDeclassement,UserVu,DateVu,DateCession,UserCession
                                                    FROM Immo LEFT JOIN ImmoObservation ON Immo.IdLastObservation = ImmoObservation.Id";
        public static string SelectAllActive { get; } = @"SELECT Immo.Id,CodeADM,CodeImmo,IdArticle,IdLocal,DateCreation,UserCreation,QrCode,IdEtiquette,IsActive,LastEtat,IdLastObservation,LastAnneeComptable,Responsable,Inventorieur,DateInventaire,ImmoExist,ImmoObservation.Observation as LastObservation,IdImmoParent,IdImmoPrincipal,Immo.Observation,DateMisEnService,UserMisEnService,DateDeclassement,UserDeclassement,UserVu,DateVu,DateCession,UserCession
                                                          FROM Immo LEFT JOIN ImmoObservation ON Immo.IdLastObservation = ImmoObservation.Id
                                                          WHERE IsActive = 1";
        public static string SelectByCode { get; } = @"SELECT Immo.Id,CodeADM,CodeImmo,IdArticle,IdLocal,Immo.DateCreation,Immo.UserCreation,QrCode,IdEtiquette,Immo.IsActive,LastEtat,IdLastObservation,LastAnneeComptable,Responsable,Inventorieur,DateInventaire,ImmoExist,ImmoObservation.Observation as LastObservation,IdImmoParent,IdImmoPrincipal,Immo.Observation,DateMisEnService,UserMisEnService,DateDeclassement,UserDeclassement,UserVu,DateVu,DateCession,UserCession
                                                       FROM Immo LEFT JOIN ImmoObservation ON Immo.IdLastObservation = ImmoObservation.Id
                                                                 LEFT JOIN Article ON Immo.IdArticle = Article.Id
                                                       WHERE CodeADM LIKE @Code OR CodeImmo LIKE @Code OR Designation LIKE @Code OR IdEtiquette=IdEtiquette";
        public static string SelectActiveByCode { get; } = @"SELECT Immo.Id,CodeADM,CodeImmo,IdArticle,IdLocal,Immo.DateCreation,Immo.UserCreation,QrCode,IdEtiquette,Immo.IsActive,LastEtat,IdLastObservation,LastAnneeComptable,Responsable,Inventorieur,DateInventaire,ImmoExist,ImmoObservation.Observation as LastObservation,IdImmoParent,IdImmoPrincipal,Immo.Observation,DateMisEnService,UserMisEnService,DateDeclassement,UserDeclassement,UserVu,DateVu,DateCession,UserCession
                                                             FROM Immo LEFT JOIN ImmoObservation ON Immo.IdLastObservation = ImmoObservation.Id
                                                                       LEFT JOIN Article ON Immo.IdArticle = Article.Id
                                                             WHERE Immo.IsActive = 1 AND (CodeADM LIKE @Code OR CodeImmo LIKE @Code OR Designation LIKE @Code )";

        public static string SelectByLocal { get; } = @"SELECT Immo.Id,CodeADM,CodeImmo,IdArticle,IdLocal,DateCreation,UserCreation,QrCode,IdEtiquette,IsActive,LastEtat,IdLastObservation,LastAnneeComptable,Responsable,Inventorieur,DateInventaire,ImmoExist,ImmoObservation.Observation as LastObservation,IdImmoParent,IdImmoPrincipal,Immo.Observation,DateMisEnService,UserMisEnService,DateDeclassement,UserDeclassement,UserVu,DateVu,DateCession,UserCession
                                                        FROM Immo LEFT JOIN ImmoObservation ON Immo.IdLastObservation = ImmoObservation.Id
                                                        WHERE Immo.IdLocal = @IdLocal AND Immo.IsActive = 1";

        public static string SelectByLocalAndArticle { get; } = @"SELECT Immo.Id,CodeADM,CodeImmo,IdArticle,IdLocal,DateCreation,UserCreation,QrCode,IdEtiquette,IsActive,LastEtat,IdLastObservation,LastAnneeComptable,Responsable,Inventorieur,DateInventaire,ImmoExist,ImmoObservation.Observation as LastObservation,IdImmoParent,IdImmoPrincipal,Immo.Observation,DateMisEnService,UserMisEnService,DateDeclassement,UserDeclassement,UserVu,DateVu,DateCession,UserCession
                                                                  FROM Immo LEFT JOIN ImmoObservation ON Immo.IdLastObservation = ImmoObservation.Id
                                                                  WHERE Immo.IdLocal = @IdLocal AND IdArticle = @IdArticle AND Immo.IsActive = 1 ";

        public static string SelectImmoPrincipalByLocal { get; } = @"SELECT Immo.Id,CodeADM,CodeImmo,IdArticle,IdLocal,DateCreation,UserCreation,QrCode,IdEtiquette,IsActive,LastEtat,IdLastObservation,LastAnneeComptable,Responsable,Inventorieur,DateInventaire,ImmoExist,ImmoObservation.Observation as LastObservation,IdImmoParent,IdImmoPrincipal,Immo.Observation,DateMisEnService,UserMisEnService,DateDeclassement,UserDeclassement,UserVu,DateVu,DateCession,UserCession
                                                                   FROM Immo LEFT JOIN ImmoObservation ON Immo.IdLastObservation = ImmoObservation.Id
                                                                   WHERE Immo.IdLocal = @IdLocal AND Immo.Id = IdImmoPrincipal";

        public static string SelectByOrgane { get; } = @"SELECT Immo.Id,CodeADM,CodeImmo,IdArticle,IdLocal,Immo.DateCreation,Immo.UserCreation,Immo.QrCode,Immo.IdEtiquette,Immo.IsActive,LastEtat,IdLastObservation,LastAnneeComptable,Responsable,Inventorieur,DateInventaire,ImmoExist,ImmoObservation.Observation as LastObservation,IdImmoParent,IdImmoPrincipal,Immo.Observation,DateMisEnService,UserMisEnService,DateDeclassement,UserDeclassement,UserVu,DateVu,DateCession,UserCession
                                                         FROM Immo LEFT JOIN ImmoObservation ON Immo.IdLastObservation = ImmoObservation.Id
		                                                           LEFT JOIN Local ON Immo.IdLocal = Local.Id
		                                                           LEFT JOIN Organe ON Local.CodeOrgane = Organe.Id
                                                         WHERE Immo.IsActive = 1 AND Organe.Id = @CodeOrgane";

        public static string SelectAllByEntite { get; } = @"SELECT Immo.Id,CodeADM,CodeImmo,IdArticle,IdLocal,Immo.DateCreation,Immo.UserCreation,Immo.QrCode,Immo.IdEtiquette,Immo.IsActive,LastEtat,IdLastObservation,LastAnneeComptable,Responsable,Inventorieur,DateInventaire,ImmoExist,ImmoObservation.Observation as LastObservation,IdImmoParent,IdImmoPrincipal,Immo.Observation,DateMisEnService,UserMisEnService,DateDeclassement,UserDeclassement,UserVu,DateVu,DateCession,UserCession
                                                         FROM Immo LEFT JOIN ImmoObservation ON Immo.IdLastObservation = ImmoObservation.Id
	                                                               LEFT JOIN Local ON Immo.IdLocal = Local.Id
                                                                   LEFT JOIN Organe ON Local.CodeOrgane = Organe.Id
                                                         WHERE Immo.IsActive = 1 ";
        public static string SelectByEntite { get; } = @"SELECT Immo.Id,CodeADM,CodeImmo,IdArticle,IdLocal,Immo.DateCreation,Immo.UserCreation,Immo.QrCode,Immo.IdEtiquette,Immo.IsActive,LastEtat,IdLastObservation,LastAnneeComptable,Responsable,Inventorieur,DateInventaire,ImmoExist,ImmoObservation.Observation as LastObservation,IdImmoParent,IdImmoPrincipal,Immo.Observation,DateMisEnService,UserMisEnService,DateDeclassement,UserDeclassement,UserVu,DateVu,DateCession,UserCession
                                                         FROM Immo LEFT JOIN ImmoObservation ON Immo.IdLastObservation = ImmoObservation.Id
	                                                               LEFT JOIN Local ON Immo.IdLocal = Local.Id
                                                                   LEFT JOIN Organe ON Local.CodeOrgane = Organe.Id
                                                         WHERE Immo.IsActive = 1 AND Organe.IdStructure =  @CodeOrgane";

        public static string SelectByResponsable { get; } = @"SELECT Immo.Id,CodeADM,CodeImmo,IdArticle,IdLocal,DateCreation,UserCreation,QrCode,IdEtiquette,IsActive,LastEtat,IdLastObservation,LastAnneeComptable,Responsable,Inventorieur,DateInventaire,ImmoExist,ImmoObservation.Observation as LastObservation,IdImmoParent,IdImmoPrincipal,Immo.Observation,DateMisEnService,UserMisEnService,DateDeclassement,UserDeclassement,UserVu,DateVu,DateCession,UserCession
                                                              FROM Immo LEFT JOIN ImmoObservation ON Immo.IdLastObservation = ImmoObservation.Id
                                                              WHERE IsActive = 1 AND Responsable = @Responsable";
        public static string SelectLitigeByResponsable { get; } = @"SELECT Immo.Id,CodeADM,CodeImmo,IdArticle,IdLocal,DateCreation,UserCreation,QrCode,IdEtiquette,IsActive,LastEtat,IdLastObservation,LastAnneeComptable,Responsable,Inventorieur,DateInventaire,ImmoExist,ImmoObservation.Observation as LastObservation,IdImmoParent,IdImmoPrincipal,Immo.Observation,DateMisEnService,UserMisEnService,DateDeclassement,UserDeclassement,UserVu,DateVu,DateCession,UserCession
                                                                    FROM Immo LEFT JOIN ImmoObservation ON Immo.IdLastObservation = ImmoObservation.Id
                                                                    WHERE Immo.IsActive = 1 AND Responsable IS NOT NULL AND LEN(Responsable)>1 AND (LastEtat = 'M' OR ImmoExist = 0) AND Responsable = @Responsable";
        public static string SelectNbreBienByResponsable { get; } = @"SELECT COUNT(*) NbreBien
                                                                      FROM Immo 
                                                                      WHERE IsActive = 1 AND Responsable = @Responsable
                                                                      GROUP BY Responsable";

        public static string SelectNbreBienLitigieuxByResponsable { get; } = @"SELECT COUNT(*) NbreBien
                                                                              FROM Immo 
                                                                              WHERE IsActive = 1 AND (LastEtat = 'M' OR ImmoExist = 0) AND Responsable = @Responsable
                                                                              GROUP BY Responsable";

        public static string SelectResponsableByOrgane { get; } = @"SELECT DISTINCT Responsable
                                                                    FROM Immo LEFT JOIN Local ON Immo.IdLocal = Local.Id
	                                                                          LEFT JOIN Organe ON Local.CodeOrgane = Organe.Id
                                                                    WHERE Immo.IsActive = 1 AND Responsable IS NOT NULL AND Organe.Id = @CodeOrgane ";

        public static string SelectResponsableLitigieux { get; } = @"SELECT DISTINCT Responsable AS Matricule, count(Id) as Nbre
                                                                     FROM Immo 
                                                                     WHERE Immo.IsActive = 1 AND Responsable IS NOT NULL AND LEN(Responsable)>1 AND (LastEtat = 'M' OR ImmoExist = 0)
                                                                     Group by Responsable
                                                                     ORDER BY Nbre DESC";

        public static string SelectResponsableLitigieuxByOrgane { get; } = @"SELECT DISTINCT Responsable AS Matricule, count(Id) as Nbre
                                                                             FROM Immo 
                                                                             WHERE Immo.IsActive = 1 AND Responsable IS NOT NULL AND LEN(Responsable)>1 AND (LastEtat = 'M' OR ImmoExist = 0) AND Organe.Id = @CodeOrgane
                                                                             Group by Responsable
                                                                             ORDER BY Nbre DESC";

        public static string SelectSansLocal { get; } = @"SELECT Immo.Id,CodeADM,CodeImmo,IdArticle,IdLocal,DateCreation,UserCreation,QrCode,IdEtiquette,IsActive,LastEtat,IdLastObservation,LastAnneeComptable,Responsable,Inventorieur,DateInventaire,ImmoExist,ImmoObservation.Observation as LastObservation,IdImmoParent,IdImmoPrincipal,Immo.Observation,DateMisEnService,UserMisEnService,DateDeclassement,UserDeclassement,UserVu,DateVu,DateCession,UserCession
                                                        FROM Immo LEFT JOIN ImmoObservation ON Immo.IdLastObservation = ImmoObservation.Id
                                                        WHERE Immo.IdLocal IS NULL";
        public static string SelectById { get; } = @"SELECT Immo.Id,CodeADM,CodeImmo,IdArticle,IdLocal,DateCreation,UserCreation,QrCode,IdEtiquette,IsActive,LastEtat,IdLastObservation,LastAnneeComptable,Responsable,Inventorieur,DateInventaire,ImmoExist,ImmoObservation.Observation as LastObservation,IdImmoParent,IdImmoPrincipal,Immo.Observation,DateMisEnService,UserMisEnService,DateDeclassement,UserDeclassement,UserVu,DateVu,DateCession,UserCession
                                                     FROM Immo LEFT JOIN ImmoObservation ON Immo.IdLastObservation = ImmoObservation.Id
                                                     WHERE Immo.Id = @Id";

        public static string SelectNameById { get; } = @"SELECT CONCAT(Designation,' ',Marque,' ',Modele) As Designation
                                                         FROM Immo INNER JOIN Article ON Immo.IdArticle = Article.Id
                                                         WHERE Immo.Id = @Id";

        public static string SelectByArticle { get; } = @"SELECT Immo.Id,CodeADM,CodeImmo,IdArticle,IdLocal,DateCreation,UserCreation,QrCode,IdEtiquette,IsActive,LastEtat,IdLastObservation,LastAnneeComptable,Responsable,Inventorieur,DateInventaire,ImmoExist,ImmoObservation.Observation as LastObservation,IdImmoParent,IdImmoPrincipal,Immo.Observation,DateMisEnService,UserMisEnService,DateDeclassement,UserDeclassement,UserVu,DateVu,DateCession,UserCession
                                                     FROM Immo LEFT JOIN ImmoObservation ON Immo.IdLastObservation = ImmoObservation.Id
                                                     WHERE Immo.IdArticle = @IdArticle";
        public static string SelectQuantiteByArticle { get; } = @"SELECT count(*) as Nbre
                                                                  FROM Immo 
                                                                  WHERE Immo.IdArticle = @IdArticle AND IsActive = 1";

        public static string SelectQuantiteByLocal { get; } = @"SELECT count(*) as Nbre
                                                                FROM Immo 
                                                                WHERE Immo.IdLocal = @IdLocal AND IsActive = 1";
        public static string SelectQuantiteIdentifierByLocal { get; } = @"SELECT count(*) as Nbre
                                                                FROM Immo 
                                                                WHERE Immo.IdLocal = @IdLocal AND IsActive = 1 AND UserVu IS NOT NULL";

        public static string SelectQuantite { get; } = @"SELECT count(*) as Nbre
                                                         FROM Immo 
                                                         WHERE IsActive = 1";
        public static string SelectQuantiteExistant { get; } = @"SELECT count(*) as Nbre
                                                         FROM Immo 
                                                         WHERE IsActive = 1 AND ImmoExist = 1";
        public static string SelectQuantiteByEtat { get; } = @"SELECT count(*) as Nbre
                                                                FROM Immo 
                                                                WHERE IsActive = 1  AND LastEtat = @LastEtat";
        public static string SelectQuantiteNonVu { get; } = @"SELECT COUNT(*) Nbre
                                                              FROM Immo INNER JOIN Local ON Immo.IdLocal = Local.Id  
                                                              WHERE  Immo.ImmoExist = 1 AND Immo.IsActive = 1 AND Local.IdTypeLocal =2 ";
        public static string SelectQuantiteNonVuByAnnee { get; } = @"SELECT COUNT(*) Nbre
                                                              FROM Immo INNER JOIN Local ON Immo.IdLocal = Local.Id  
                                                              WHERE  Immo.ImmoExist = 1 AND Immo.IsActive = 1 AND Local.IdTypeLocal = 2 And LastAnneeComptable = @Annee";
        public static string SelectQuantiteIdentifie { get; } = @"SELECT count(*) as Nbre
                                                                FROM Immo 
                                                                WHERE IsActive = 1 AND UserVu IS NOT NULL";

        public static string SelectByQrCode { get; } = @"SELECT Immo.Id,CodeADM,CodeImmo,IdArticle,IdLocal,DateCreation,UserCreation,QrCode,IdEtiquette,IsActive,LastEtat,IdLastObservation,LastAnneeComptable,Responsable,Inventorieur,DateInventaire,ImmoExist,ImmoObservation.Observation as LastObservation,IdImmoParent,IdImmoPrincipal,Immo.Observation,DateMisEnService,UserMisEnService,DateDeclassement,UserDeclassement,UserVu,DateVu,DateCession,UserCession
                                                         FROM Immo LEFT JOIN ImmoObservation ON Immo.IdLastObservation = ImmoObservation.Id
                                                         WHERE Immo.QrCode = @QrCode";
        public static string SelectByIdEtiquette { get; } = @"SELECT Immo.Id,CodeADM,CodeImmo,IdArticle,IdLocal,DateCreation,UserCreation,QrCode,IdEtiquette,IsActive,LastEtat,IdLastObservation,LastAnneeComptable,Responsable,Inventorieur,DateInventaire,ImmoExist,ImmoObservation.Observation as LastObservation,IdImmoParent,IdImmoPrincipal,Immo.Observation,DateMisEnService,UserMisEnService,DateDeclassement,UserDeclassement,UserVu,DateVu,DateCession,UserCession
                                                         FROM Immo LEFT JOIN ImmoObservation ON Immo.IdLastObservation = ImmoObservation.Id
                                                         WHERE Immo.IdEtiquette = @IdEtiquette ";
        public static string SelectByLongIds { get; } = @"SELECT Immo.Id,CodeADM,CodeImmo,IdArticle,IdLocal,DateCreation,UserCreation,QrCode,IdEtiquette,IsActive,LastEtat,IdLastObservation,LastAnneeComptable,Responsable,Inventorieur,DateInventaire,ImmoExist,ImmoObservation.Observation as LastObservation,IdImmoParent,IdImmoPrincipal,Immo.Observation,DateMisEnService,UserMisEnService,DateDeclassement,UserDeclassement,UserVu,DateVu,DateCession,UserCession
                                                         FROM Immo LEFT JOIN ImmoObservation ON Immo.IdLastObservation = ImmoObservation.Id
                                                         WHERE Immo.IdEtiquette = @IdEtiquette OR Immo.Id = @IdEtiquette";
        public static string SelectPhotosById { get; } = @"SELECT Id,IdImmo,Constat
                                                          FROM ImmoPhotos
                                                          WHERE IdImmo = @Id";


        public static string AffectQRCode { get; } = @"UPDATE Immo
                                                       SET QrCode = @QrCode, IdEtiquette = @IdEtiquette
                                                       WHERE Id = @Id";
        public static string DesaffectQRCode { get; } = @"UPDATE Immo
                                                       SET QrCode = NULL, IdEtiquette = NULL
                                                       WHERE Id = @Id";

        public static string Insert { get; } = @"spImmo_InsertMultiple";
        public static string ImmoAndInventaireDetailsInsert { get; } = @"spImmoAndInventaireDetails_Insert";

        public static string InsertPhoto { get; } = @"INSERT INTO ImmoPhotos (Id,IdImmo,Constat)
                                                      VALUES (@Id,@IdImmo,@Constat)";
        public static string UpdatePhoto { get; } = @"UPDATE ImmoPhotos 
                                                      SET Constat = @Constat
                                                      WHERE Id = @Id";
        public static string ChangeLocal { get; } = @"UPDATE Immo 
                                                      SET IdLocal = @IdLocal, Observation = @DesignationLocal
                                                      WHERE Id = @Id";

        public static string MisEnService { get; } = @"UPDATE Immo 
                                                      SET IdLocal = @IdLocal, DateMisEnService = GETDATE(), UserMisEnService = @UserMisEnService
                                                      WHERE Id = @Id";

        public static string Update { get; } = @"UPDATE Immo
                                                 SET CodeImmo = @CodeImmo, IdArticle = @IdArticle,IdLocal = @IdLocal, IsActive = @IsActive, Responsable = @Responsable,UserCreation = @UserCreation,DateCreation = @DateCreation,LastEtat = @LastEtat, IdLastObservation = @IdLastObservation,IdImmoParent = @IdImmoParent,IdImmoPrincipal = @IdImmoPrincipal
                                                 WHERE Id = @Id";
        public static string Inventaire { get; } = @"UPDATE Immo
                                                   SET LastEtat = @Etat, IdLastObservation = @IdObservation, LastAnneeComptable = @Annee, Responsable = @Responsable,Inventorieur = @UserCreation,DateInventaire = getdate(),ImmoExist = @ImmoExist
                                                   WHERE Id = @IdImmo AND Immo.UserVu IS NOT NULL";
        public static string InventaireLocal { get; } = @"UPDATE Immo
                                                        SET LastAnneeComptable = @Annee,  Inventorieur = @UserCreation,DateInventaire = getdate()
                                                        WHERE Immo.IdLocal = @IdLocal AND Immo.IsActive = 1 AND QrCode IS NOT NULL AND Immo.UserVu IS NOT NULL AND ImmoExist = 1 AND (Inventorieur IS NULL OR LastAnneeComptable != @Annee) ";
        public static string InventaireNonVu { get; } = @"UPDATE Immo
                                                         SET LastEtat = NULL, IdLastObservation = NULL, LastAnneeComptable = @Annee, Inventorieur = @UserCreation,DateInventaire = getdate(),ImmoExist = 0, Observation = @Observation
                                                         WHERE Id = @IdImmo";
        public static string InventaireReset { get; } = @"UPDATE Immo
                                                          SET UserVu = NULL, DateVu = NULL, Inventorieur = NULL, DateInventaire = NULL";
        public static string Declassement { get; } = @"UPDATE Immo
                                                       SET IdLastObservation = 3, DateDeclassement = getdate(), UserDeclassement = @UserDeclassement, LastAnneeComptable = @Annee
                                                       WHERE Id = @Id";
        public static string Cession { get; } = @"UPDATE Immo
                                                       SET IsActive = 0, DateCession = getdate(), UserCession = @UserDeclassement
                                                       WHERE Id = @IdImmo";
        public static string CessionLocal { get; } = @"UPDATE Immo
                                                       SET IsActive = 0, DateCession = getdate(), UserCession = @UserDeclassement
                                                       WHERE IdLocal = @IdLocal";
        public static string Delete { get; } = @"DELETE FROM Immo WHERE Id = @Id";
        public static string DeletePhoto { get; } = @"DELETE FROM ImmoPhotos WHERE Id = @Id";

        public static string IdentifierImmo { get; } = @"UPDATE Immo
                                                       SET LastEtat = @LastEtat, IdLastObservation = @IdLastObservation, UserVu = @UserVu, DateVu =  getdate(), IsActive = 1, ImmoExist = 1
                                                       WHERE Id = @IdImmo";
    }
}