using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlCommande
    {
        public static string SelectAll { get; } = @"SELECT Id,NumeroCommande,NbrePage,IdFormat,CreatedBy,DateCreation,ValidateBy,DateValidation,PrintBy,DatePrint
                                                    FROM Commande
                                                    WHERE PrintBy IS NULL
                                                    ORDER BY DateCreation";

        public static string SelectAllPrint { get; } = @"SELECT Id,NumeroCommande,NbrePage,IdFormat,CreatedBy,DateCreation,ValidateBy,DateValidation,PrintBy,DatePrint
                                                    FROM Commande
                                                    WHERE PrintBy IS NOT NULL
                                                    ORDER BY DateCreation";
        public static string SelectById { get; } = @"SELECT Id,NumeroCommande,NbrePage,IdFormat,CreatedBy,DateCreation,ValidateBy,DateValidation,PrintBy,DatePrint
                                                    FROM Commande
                                                    WHERE Id = @Id AND PrintBy IS NULL";
        public static string SelectPrintById { get; } = @"SELECT Id,NumeroCommande,NbrePage,IdFormat,CreatedBy,DateCreation,ValidateBy,DateValidation,PrintBy,DatePrint
                                                    FROM Commande
                                                    WHERE Id = @Id AND PrintBy IS NOT NULL";
        public static string SelectNbreEtiquette { get; } = @"SELECT Count(*) as Nbre
                                                              FROM Etiquette
                                                              WHERE IdCommande = @IdCommande";
        public static string SelectNbreEtiquetteUsed { get; } = @"SELECT Count(*) as Nbre
                                                              FROM Etiquette
                                                              WHERE IdCommande = @IdCommande AND IsUsed = 1";
        public static string SelectNbreEtiquetteNotUsed { get; } = @"SELECT Count(*) as Nbre
                                                              FROM Etiquette
                                                              WHERE IdCommande = @IdCommande AND IsUsed = 0";
        public static string Insert { get; } = @"INSERT INTO Commande(NbrePage,IdFormat,CreatedBy,DateCreation)
                                                 VALUES (@NbrePage,@IdFormat,@CreatedBy,GetDate())";

        public static string Update { get; } = @"UPDATE Commande
                                                 SET NbrePage = @NbrePage ,IdFormat = @IdFormat, CreatedBy = @CreatedBy
                                                 WHERE Id = @Id";

        public static string Validate { get; } = @"UPDATE Commande
                                                 SET ValidateBy = @ValidateBy, DateValidation = GetDate()
                                                 WHERE Id = @Id";
        public static string Print { get; } = @"UPDATE Commande
                                                 SET PrintBy = @PrintBy, DatePrint = GetDate()
                                                 WHERE Id = @Id";

        public static string InsertUsingStoredProcedure { get; } = @"spCommande_Insert";

    }
}