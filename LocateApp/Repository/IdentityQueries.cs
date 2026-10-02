using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlUtilisateur
    {
        public static string SelectAll { get; } = @"SELECT IdUser,UserName,Password,Nom,Postnom,Prenom,Sexe,Adresse,Telephone,Email,IdProfil,
                                                           IsExternalUser, IsLocked,NbreTentatives,FirstConnexion,IdInstitution,CodeOrgane,FocalPoint,DateCreation,LastConnexion
                                                    FROM _Utilisateur
                                                    WHERE Actif = 1";

        public static string SelectAllDesable { get; } = @"SELECT IdUser,UserName,Password,Nom,Postnom,Prenom,Sexe,Adresse,Telephone,Email,IdProfil,
                                                                  IsExternalUser, IsLocked,NbreTentatives,FirstConnexion,IdInstitution,CodeOrgane,FocalPoint,DateCreation,LastConnexion
                                                           FROM _Utilisateur
                                                           WHERE Actif = 0";

        public static string SelectById { get; } = @"SELECT IdUser,UserName,Password,Nom,Postnom,Prenom,Sexe,Adresse,Telephone,Email,IdProfil,
                                                            IsExternalUser, IsLocked,NbreTentatives,FirstConnexion,IdInstitution,CodeOrgane,FocalPoint,DateCreation,LastConnexion
                                                     FROM _Utilisateur
                                                     WHERE IdUser = @IdUser AND Actif = 1";
        public static string SelectAllById { get; } = @"SELECT IdUser,UserName,Password,Nom,Postnom,Prenom,Sexe,Adresse,Telephone,Email,IdProfil,
                                                               IsExternalUser, IsLocked,NbreTentatives,FirstConnexion,IdInstitution,CodeOrgane,FocalPoint,DateCreation,LastConnexion
                                                        FROM _Utilisateur
                                                        WHERE UserName = @UserName";
        public static string SelectPassword { get; } = @"SELECT Password
                                                         FROM _Utilisateur
                                                         WHERE IdUser = @IdUser";
        public static string SelectFocalPoint { get; } = @"SELECT IdUser,UserName,Password,Nom,Postnom,Prenom,Sexe,Adresse,Telephone,Email,IdProfil,
                                                                  IsExternalUser, IsLocked,NbreTentatives,FirstConnexion,IdInstitution,CodeOrgane,FocalPoint,DateCreation,LastConnexion
                                                           FROM _Utilisateur
                                                           WHERE FocalPoint = 1";
        public static string SelectByUserName { get; } = @"SELECT IdUser,UserName,Password,Nom,Postnom,Prenom,Sexe,Adresse,Telephone,Email,IdProfil,
                                                                  IsExternalUser, IsLocked,NbreTentatives,FirstConnexion,IdInstitution,CodeOrgane,FocalPoint,DateCreation,LastConnexion
                                                           FROM _Utilisateur
                                                           WHERE UserName = @UserName AND Actif = 1";
        public static string Insert { get; } = @"INSERT INTO _Utilisateur (IdUser,UserName,Password,Nom,Postnom,Prenom,Sexe,Adresse,Telephone,Email,IdProfil,IsExternalUser,FirstConnexion,IdInstitution,CodeOrgane)
                                                 VALUES (@IdUser, @UserName, @Password, @Nom, @Postnom, @Prenom, @Sexe, @Adresse, @Telephone, @Email, @IdProfil,@IsExternalUser,@FirstConnexion,@IdInstitution, @CodeOrgane)";
        public static string Update { get; } = @"UPDATE _Utilisateur
                                                 SET Nom = @Nom, Postnom = @Postnom, Prenom = @Prenom, Sexe = @Sexe, Adresse = @Adresse, 
                                                     Telephone = @Telephone, Email = @Email, IdProfil = @IdProfil, IsExternalUser = @IsExternalUser, 
                                                     IsLocked = @IsLocked, IdInstitution = @IdInstitution
                                                 WHERE UserName = @UserName";
        public static string UpdateBcc { get; } = @"UPDATE _Utilisateur
                                                 SET  Adresse = @Adresse, Telephone = @Telephone, Email = @Email, IdProfil = @IdProfil, 
                                                      IsExternalUser = @IsExternalUser, IsLocked = @IsLocked, FocalPoint = @FocalPoint,
                                                      IdInstitution = @IdInstitution, CodeOrgane = @CodeOrgane
                                                 WHERE UserName = @UserName";
        public static string UpdatePassword { get; } = @"UPDATE _Utilisateur
                                                         SET Password = @Password, FirstConnexion = 0
                                                         WHERE IdUser = @IdUser";
        public static string UpdateTentative { get; } = @"UPDATE _Utilisateur
                                                         SET NbreTentatives += 1
                                                         WHERE IdUser = @IdUser";
        public static string UpdateResetTentative { get; } = @"UPDATE _Utilisateur
                                                         SET NbreTentatives = 0
                                                         WHERE IdUser = @IdUser";
        public static string Lock { get; } = @"UPDATE _Utilisateur
                                               SET IsLocked = 1
                                               WHERE UserName = @UserName";
        public static string Desactiver { get; } = @"UPDATE _Utilisateur
                                                   SET Actif = 0
                                                   WHERE UserName = @UserName";

        public static string Activer { get; } = @"UPDATE _Utilisateur
                                                  SET Actif = 1
                                                  WHERE UserName = @UserName";
        public static string ResetPassword { get; } = @"UPDATE _Utilisateur
                                                        SET Password = @Password, IsLocked = 0, NbreTentatives = 0, FirstConnexion = 1, Actif = 1
                                                        WHERE IdUser = @IdUser";
        public static string LastConnexion { get; } = @"UPDATE _Utilisateur
                                                        SET LastConnexion = getdate()
                                                         WHERE IdUser = @IdUser";
    }
}