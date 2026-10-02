using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlEtiquette
    {
        public static string SelectAll { get; } = @"SELECT Etiquette.Id,QrCode,IdCommande,IdFormat,DateCreation,UserCreation,Printed,DatePrint,UserPrint,IsUsed,UsedBy,DateUsed,EtiquetteType.Nom AS TypeUsed
                                                    FROM Etiquette LEFT JOIN EtiquetteType ON Etiquette.IdEtiquetteType = EtiquetteType.Id
                                                    WHERE IdCommande = @IdCommande";
        public static string SelectByQrCode { get; } = @"SELECT Etiquette.Id,QrCode,IdCommande,IdFormat,DateCreation,UserCreation,Printed,DatePrint,UserPrint,IsUsed,UsedBy,DateUsed,EtiquetteType.Nom AS TypeUsed
                                                         FROM Etiquette LEFT JOIN EtiquetteType ON Etiquette.IdEtiquetteType = EtiquetteType.Id
                                                         WHERE QrCode = @QrCode";
        public static string SelectPrintedByQrCode { get; } = @"SELECT Etiquette.Id,QrCode,IdCommande,IdFormat,DateCreation,UserCreation,Printed,DatePrint,UserPrint,IsUsed,UsedBy,DateUsed,EtiquetteType.Nom AS TypeUsed
                                                                FROM Etiquette LEFT JOIN EtiquetteType ON Etiquette.IdEtiquetteType = EtiquetteType.Id
                                                                WHERE Printed = 1 AND QrCode = @QrCode";
        public static string SelectUnusedPrinted { get; } = @"SELECT Etiquette.Id,QrCode,IdCommande,IdFormat,DateCreation,UserCreation,Printed,DatePrint,UserPrint,IsUsed,UsedBy,DateUsed,EtiquetteType.Nom AS TypeUsed
                                                    FROM Etiquette LEFT JOIN EtiquetteType ON Etiquette.IdEtiquetteType = EtiquetteType.Id
                                                    WHERE Printed = 1 AND IsUsed = 0";
        public static string SelectById { get; } = @"SELECT Etiquette.Id,QrCode,IdCommande,IdFormat,DateCreation,UserCreation,Printed,DatePrint,UserPrint,IsUsed,UsedBy,DateUsed,EtiquetteType.Nom AS TypeUsed
                                                    FROM Etiquette LEFT JOIN EtiquetteType ON Etiquette.IdEtiquetteType = EtiquetteType.Id
                                                    WHERE Id = @Id";
        public static string Insert { get; } = @"INSERT INTO Etiquette(IdCommande,IdFormat,UserCreation)
                                                 VALUES (@IdCommande,@IdFormat,@UserCreation)";
        public static string UseIt { get; } = @"UPDATE Etiquette
                                                SET IsUsed = 1, UsedBy = @UsedBy, DateUsed = getdate(),IdEtiquetteType = @IdEtiquetteType
                                                WHERE QrCode = @QrCode";
        public static string LibereIt { get; } = @"UPDATE Etiquette
                                                SET IsUsed = 0, UsedBy = NULL, DateUsed = NULL,IdEtiquetteType = NULL
                                                WHERE QrCode = @QrCode";

        public static string Print { get; } = @"UPDATE Etiquette
                                                SET Printed = 1, UserPrint = @UserPrint, DatePrint = getdate()
                                                WHERE IdCommande = @IdCommande";

        public static string Delete { get; } = @"DELETE FROM Etiquette
                                                WHERE IdCommande = @IdCommande";

        public static string InsertUsingStoredProcedure { get; } = @"spEtiquette_Insert";
    }
}