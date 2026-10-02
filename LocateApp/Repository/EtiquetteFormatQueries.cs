using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlEtiquetteFormat
    {
        public static string SelectAll { get; } = @"SELECT Id,Nom,Colone,Ligne,Largeur,Hauteur,Papier,IsPublic
                                                    FROM EtiquetteFormat";
        public static string SelectAllPublic { get; } = @"SELECT Id,Nom,Colone,Ligne,Largeur,Hauteur,Papier,IsPublic
                                                    FROM EtiquetteFormat
                                                    WHERE IsPublic = 1";
        public static string SelectById { get; } = @"SELECT Id,Nom,Colone,Ligne,Largeur,Hauteur,Papier,IsPublic
                                                    FROM EtiquetteFormat
                                                    WHERE Id = @Id";
        public static string Insert { get; } = @"INSERT INTO EtiquetteFormat(Nom,Colone,Ligne,Largeur,Hauteur,Papier,IsPublic)
                                                 VALUES (@Nom,@Colone,@Ligne,@Largeur,@Hauteur,@Papier,@IsPublic)";

        public static string Update { get; } = @"UPDATE EtiquetteFormat 
                                                 SET Nom = @Nom, Colone = @Colone, Ligne = @Ligne, Largeur = @Largeur, Hauteur = @Hauteur, Papier = @Papier, IsPublic = @IsPublic
                                                 WHERE Id = @Id";
        public static string Delete { get; } = @"DELETE FROM EtiquetteFormat                                                  
                                                 WHERE Id = @Id";
    }
}