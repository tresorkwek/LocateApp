using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlProfil
    {
        public static string SelectAll { get; } = @"SELECT IdProfil,Nom,Libelle,Root,DefaultProfil,Externe,HomeUrl
                                                    FROM _Profil";
        public static string SelectAllWithoutClaims { get; } = @"SELECT IdProfil,Nom,Libelle,DefaultProfil,Externe,HomeUrl
                                                                 FROM _Profil
                                                                 WHERE Root = 0 AND IdProfil NOT IN (SELECT IdProfil FROM _Claim)";
        public static string SelectAllWithClaims { get; } = @"SELECT IdProfil,Nom,Libelle,DefaultProfil,Externe,HomeUrl
                                                              FROM _Profil
                                                              WHERE Root = 0 AND IdProfil IN (SELECT IdProfil FROM _Claim)";
        public static string SelectAllWithClaimsByProfil { get; } = @"SELECT IdProfil,Nom,Libelle,DefaultProfil,Externe,HomeUrl
                                                                     FROM _Profil
                                                                     WHERE Root = 0 AND IdProfil IN (SELECT IdProfil FROM _Claim)
                                                                           AND IdProfil = @IdProfil";

        public static string SelectById { get; } = @"SELECT IdProfil,Nom,Libelle,Root,DefaultProfil,Externe,HomeUrl
                                                     FROM _Profil
                                                     WHERE IdProfil = @IdProfil";
        public static string SelectDefault { get; } = @"SELECT IdProfil,Nom,Libelle,Root,DefaultProfil,Externe,HomeUrl
                                                     FROM _Profil
                                                     WHERE DefaultProfil = 1";
        public static string Insert { get; } = @"INSERT INTO _Profil(Nom,Libelle,Externe,HomeUrl)
                                                 VALUES (@Nom,@Libelle,@Externe,@HomeUrl)";
        public static string Update { get; } = @"UPDATE _Profil
                                                 SET Nom = @Nom, Libelle = @Libelle, Externe = @Externe, HomeUrl = @HomeUrl
                                                 WHERE IdProfil = @IdProfil";

    }
}