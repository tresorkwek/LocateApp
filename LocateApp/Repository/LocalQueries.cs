using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlLocal
    {
        public static string SelectAll { get; } = @"SELECT Local.Id,Code,Designation,CodeOrgane,QrCode,IdEtiquette,IsSpace,IsActive,DateCreation,UserCreation,IdTypeLocal
                                                    FROM Local LEFT JOIN Organe ON Local.CodeOrgane = Organe.Id
                                                    ORDER BY Organe.Ordre,Organe.Id";
        public static string SelectAllLocal { get; } = @"SELECT Local.Id,Code,Designation,CodeOrgane,QrCode,IdEtiquette,IsSpace,IsActive,DateCreation,UserCreation,IdTypeLocal
                                                        FROM Local LEFT JOIN Organe ON Local.CodeOrgane = Organe.Id
                                                         WHERE IsSpace = 0 AND IdTypeLocal = 1 
                                                         ORDER BY Organe.Ordre,Organe.Id";

        public static string SelectAllSpace { get; } = @"SELECT Id,Code,Designation,CodeOrgane,QrCode,IdEtiquette,IsSpace,IsActive,DateCreation,UserCreation,IdTypeLocal
                                                         FROM Local
                                                         WHERE IsSpace = 1 AND IdTypeLocal = 1
                                                         ORDER BY CodeOrgane";

        public static string SelectAllActive { get; } = @"SELECT Local.Id,Code,Designation,CodeOrgane,QrCode,IdEtiquette,IsSpace,IsActive,DateCreation,UserCreation,IdTypeLocal
                                                          FROM Local LEFT JOIN Organe ON Local.CodeOrgane = Organe.Id
                                                          WHERE IsActive = 1 AND IdTypeLocal = 1
                                                          ORDER BY Organe.Ordre,Organe.Id";
        public static string SelectAllLocalActive { get; } = @"SELECT Local.Id,Code,Designation,CodeOrgane,QrCode,IdEtiquette,IsSpace,IsActive,DateCreation,UserCreation,IdTypeLocal
                                                               FROM Local LEFT JOIN Organe ON Local.CodeOrgane = Organe.Id
                                                               WHERE IsSpace = 0 AND IsActive = 1 AND IdTypeLocal = 1
                                                               ORDER BY Organe.Ordre,Organe.Id";

        public static string SelectAllActiveLocal { get; } = @"SELECT Local.Id,Code,Designation,CodeOrgane,QrCode,IdEtiquette,IsSpace,IsActive,DateCreation,UserCreation,IdTypeLocal
                                                               FROM Local LEFT JOIN Organe ON Local.CodeOrgane = Organe.Id
                                                               WHERE IsSpace = 0 AND IsActive = 1 AND IdTypeLocal = 1
                                                               ORDER BY Organe.Ordre,Organe.Id";

        public static string SelectAllActiveSpace { get; } = @"SELECT Id,Code,Designation,CodeOrgane,QrCode,IdEtiquette,IsSpace,IsActive,DateCreation,UserCreation,IdTypeLocal
                                                               FROM Local
                                                               WHERE IsSpace = 1 AND IsActive = 1 AND IdTypeLocal = 1
                                                               ORDER BY CodeOrgane";

        public static string SelectById { get; } = @"SELECT Id,Code,Designation,CodeOrgane,QrCode,IdEtiquette,IsSpace,IsActive,DateCreation,UserCreation,IdTypeLocal
                                                     FROM Local
                                                     WHERE Id = @Id";
        public static string SelectByOrgane { get; } = @"SELECT Id,Code,Designation,CodeOrgane,QrCode,IdEtiquette,IsSpace,IsActive,DateCreation,UserCreation,IdTypeLocal
                                                        FROM Local
                                                        WHERE CodeOrgane LIKE @CodeOrgane AND IdTypeLocal = 1 AND IsActive = 1";
        public static string SelectLocalByOrgane { get; } = @"SELECT Id,Code,Designation,CodeOrgane,QrCode,IdEtiquette,IsSpace,IsActive,DateCreation,UserCreation,IdTypeLocal
                                                              FROM Local
                                                              WHERE CodeOrgane LIKE @CodeOrgane AND IdTypeLocal = 1 AND IsActive = 1";
        public static string SelectByQrCode { get; } = @"SELECT Id,Code,Designation,CodeOrgane,QrCode,IdEtiquette,IsSpace,IsActive,DateCreation,UserCreation,IdTypeLocal
                                                         FROM Local
                                                         WHERE QrCode = @QrCode AND IdTypeLocal = 1";
        public static string SelectNonVu { get; } = @"SELECT Id,Code,Designation,CodeOrgane,QrCode,IdEtiquette,IsSpace,IsActive,DateCreation,UserCreation,IdTypeLocal
                                                      FROM Local
                                                      WHERE IdTypeLocal = 2 AND CodeOrgane = @CodeOrgane";
        public static string SelectDeclasser { get; } = @"SELECT Id,Code,Designation,CodeOrgane,QrCode,IdEtiquette,IsSpace,IsActive,DateCreation,UserCreation,IdTypeLocal
                                                          FROM Local
                                                          WHERE IdTypeLocal = 4 AND CodeOrgane = @CodeOrgane";
        public static string SelectTransit { get; } = @"SELECT Id,Code,Designation,CodeOrgane,QrCode,IdEtiquette,IsSpace,IsActive,DateCreation,UserCreation,IdTypeLocal
                                                      FROM Local
                                                      WHERE IdTypeLocal = 3 AND CodeOrgane = @CodeOrgane";
        public static string SelectByIdEtiquette { get; } = @"SELECT Id,Code,Designation,CodeOrgane,QrCode,IdEtiquette,IsSpace,IsActive,DateCreation,UserCreation,IdTypeLocal
                                                              FROM Local
                                                              WHERE IdEtiquette = @IdEtiquette AND IdTypeLocal = 1";
        public static string SelectSpaceByQrCode { get; } = @"SELECT Id,Code,Designation,CodeOrgane,QrCode,IdEtiquette,IsSpace,IsActive,DateCreation,UserCreation,IdTypeLocal
                                                              FROM Local
                                                              WHERE IsSpace = 1 AND QrCode = @QrCode AND IdTypeLocal = 1";
        public static string SelectSpaceByIdEtiquette { get; } = @"SELECT Id,Code,Designation,CodeOrgane,QrCode,IdEtiquette,IsSpace,IsActive,DateCreation,UserCreation,IdTypeLocal
                                                                   FROM Local
                                                                   WHERE IsSpace = 1 AND IdEtiquette = @IdEtiquette AND IdTypeLocal = 1";
        public static string SelectByName { get; } = @"SELECT Local.Id,Code,Designation,CodeOrgane,QrCode,IdEtiquette,IsSpace,IsActive,DateCreation,UserCreation,IdTypeLocal
                                                       FROM Local LEFT JOIN Organe ON Local.CodeOrgane = Organe.Id
                                                       WHERE Designation LIKE @Designation AND IdTypeLocal = 1 AND IsActive = 1 
                                                       ORDER BY Organe.Ordre,Organe.Id";
        public static string SelectLocalByName { get; } = @"SELECT Local.Id,Code,Designation,CodeOrgane,QrCode,IdEtiquette,IsSpace,IsActive,DateCreation,UserCreation,IdTypeLocal
                                                            FROM Local LEFT JOIN Organe ON Local.CodeOrgane = Organe.Id
                                                            WHERE IsSpace = 0 AND Designation LIKE @Designation AND IdTypeLocal = 1 AND IsActive = 1 
                                                            ORDER BY Organe.Ordre,Organe.Id";
        public static string SelectByNameAndOrgane { get; } = @"SELECT Id,Code,Designation,CodeOrgane,QrCode,IdEtiquette,IsSpace,IsActive,DateCreation,UserCreation,IdTypeLocal
                                                       FROM Local
                                                       WHERE (Code LIKE @Code OR Designation LIKE @Code) AND CodeOrgane = @CodeOrgane AND IdTypeLocal = 1 AND IsActive = 1 ";
        public static string SelectActiveByName { get; } = @"SELECT Local.Id,Code,Designation,CodeOrgane,QrCode,IdEtiquette,IsSpace,IsActive,DateCreation,UserCreation,IdTypeLocal
                                                             FROM Local LEFT JOIN Organe ON Local.CodeOrgane = Organe.Id
                                                             WHERE IsActive = 1 AND  Designation LIKE @Designation AND IdTypeLocal = 1
                                                             ORDER BY Organe.Ordre,Organe.Id";
        public static string SelectLocalActiveByName { get; } = @"SELECT Local.Id,Code,Designation,CodeOrgane,QrCode,IdEtiquette,IsSpace,IsActive,DateCreation,UserCreation,IdTypeLocal
                                                                  FROM Local LEFT JOIN Organe ON Local.CodeOrgane = Organe.Id
                                                                  WHERE IsSpace = 0 AND IsActive = 1 AND  Designation LIKE @Designation AND IdTypeLocal = 1
                                                                  ORDER BY Organe.Ordre,Organe.Id";
        public static string SelectQuantite { get; } = @"SELECT count(*) as Nbre
                                                         FROM Local 
                                                         WHERE IsActive = 1 AND IdTypeLocal = 1";

        public static string SelectQuantiteIdentifie { get; } = @"SELECT count(*) as Nbre
                                                                FROM Local 
                                                                WHERE IsActive = 1 AND QrCode IS NOT NULL AND IdTypeLocal = 1";

        public static string AffectQRCode { get; } = @"UPDATE Local
                                                       SET QrCode = @QrCode, IdEtiquette = @IdEtiquette
                                                       WHERE Id = @Id";
        public static string DesaffectQRCode { get; } = @"UPDATE Local
                                                       SET QrCode = NULL, IdEtiquette = NULL
                                                       WHERE Id = @Id";

        public static string Insert { get; } = @"INSERT INTO Local (Code,Designation,CodeOrgane,IsSpace,UserCreation,IdTypeLocal)
                                                 VALUES (@Code,@Designation,@CodeOrgane,@IsSpace,@UserCreation,@IdTypeLocal)";

        public static string InsertSyncStoredProcedure { get; } = @"spLocal_Insert";


        public static string InsertSync { get; } = @"INSERT INTO Local (Code,Designation,CodeOrgane,IsSpace,UserCreation,QrCode,IdEtiquette)
                                                     VALUES (@Code,@Designation,@CodeOrgane,@IsSpace,@UserCreation,
                                                            CASE WHEN @IdEtiquette IN (-1, 0, 1) THEN NULL ELSE @QrCode END,
                                                            CASE WHEN @IdEtiquette IN (-1, 0, 1) THEN NULL ELSE @IdEtiquette END)";

        public static string InsertSyncSaved { get; } = @"INSERT INTO Local (Code,Designation,CodeOrgane,IsSpace,UserCreation,QrCode,IdEtiquette)
                                                     VALUES (@Code,@Designation,@CodeOrgane,@IsSpace,@UserCreation,@QrCode,@IdEtiquette)";

        public static string Update { get; } = @"UPDATE Local 
                                                 SET Code = @Code, Designation = @Designation, CodeOrgane = @CodeOrgane, IsSpace = @IsSpace, IsActive = @IsActive, IdTypeLocal = @IdTypeLocal
                                                 WHERE Id = @Id";

        public static string Delete { get; } = @"DELETE FROM Local WHERE Id = @Id";

    }
}