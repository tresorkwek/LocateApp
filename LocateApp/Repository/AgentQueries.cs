using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository

{
    public static class SqlAgent
    {

        public static string SelectAll { get; } = @"SELECT SerialId,Matricule,Nom,Postnom,Prenom,Sexe,DateNaissance,Telephone,Email,CodeOrgane
													FROM Agent";

        public static string SelectById { get; } = @"SELECT SerialId,Matricule,Nom,Postnom,Prenom,Sexe,DateNaissance,Telephone,Email,CodeOrgane
													FROM Agent
													WHERE Matricule LIKE @Matricule";

        public static string SelectByName { get; } = @"SELECT SerialId,Matricule,Nom,Postnom,Prenom,Sexe,DateNaissance,Telephone,Email,CodeOrgane
													   FROM Agent
													   WHERE  Nom LIKE @Nom OR Postnom LIKE @Nom OR Prenom LIKE @Nom";

		// Agents d'un organe, dans la table Agent de Locate (les codes d'organe y suivent l'organigramme de Locate).
		public static string SelectByOrgane { get; } = @"SELECT SerialId,Matricule,Nom,Postnom,Prenom,Sexe,DateNaissance,Telephone,Email,CodeOrgane
													FROM Agent
													WHERE CodeOrgane = @CodeOrgane";

		// Répertoire des agents (page /agent/) : une seule requête pour l'organe, le compte et le nombre de biens de chacun.
		public static string SelectRepertoire { get; } = @"SELECT a.SerialId, a.Matricule, a.Nom, a.Postnom, a.Prenom, a.Sexe, a.DateNaissance, a.Telephone, a.Email, a.CodeOrgane,
													  o.Nom AS NomOrgane, o.IdStructure, s.Nom AS NomStructure,
													  u.UserName AS Compte, p.Libelle AS Profil, ISNULL(u.Actif, 0) AS CompteActif,
													  ISNULL(b.NbreBiens, 0) AS NbreBiens
												   FROM Agent a
													    LEFT JOIN Organe o ON o.Id = a.CodeOrgane
													    LEFT JOIN Organe s ON s.Id = o.IdStructure
													    LEFT JOIN _Utilisateur u ON u.UserName = LEFT(a.Matricule, 6)
													    LEFT JOIN _Profil p ON p.IdProfil = u.IdProfil
													    LEFT JOIN (SELECT Responsable, COUNT(*) AS NbreBiens FROM Immo
													               WHERE IsActive = 1 AND Responsable IS NOT NULL GROUP BY Responsable) b ON b.Responsable = LEFT(a.Matricule, 6)
												   ORDER BY a.Nom, a.Postnom, a.Prenom";

		// Ajout / modification d'un agent (le matricule de la table Agent est celui de l'agent suivi de « 00 »)
		public static string SelectByMatricule { get; } = @"SELECT SerialId,Matricule,Nom,Postnom,Prenom,Sexe,DateNaissance,Telephone,Email,CodeOrgane
													FROM Agent
													WHERE Matricule = @Matricule + '00'";
		public static string MatriculeUtilise { get; } = @"SELECT (SELECT COUNT(*) FROM Agent WHERE LEFT(Matricule, 6) = @Matricule)
													     + (SELECT COUNT(*) FROM _Utilisateur WHERE UserName = @Matricule)";
		public static string DernierMatricule { get; } = @"SELECT ISNULL(MAX(CAST(LEFT(Matricule, 6) AS int)), 700000)
													  FROM Agent
													  WHERE LEN(RTRIM(Matricule)) = 8 AND LEFT(Matricule, 6) NOT LIKE '%[^0-9]%'";
		public static string Insert { get; } = @"INSERT INTO Agent (SerialId, Matricule, Nom, Postnom, Prenom, Sexe, DateNaissance, Telephone, Email, CodeOrgane)
												 VALUES (@SerialId, @Matricule + '00', @Nom, @Postnom, @Prenom, @Sexe, @DateNaissance, @Telephone, @Email, @CodeOrgane)";
		public static string Update { get; } = @"UPDATE Agent
												 SET Nom = @Nom, Postnom = @Postnom, Prenom = @Prenom, Sexe = @Sexe, DateNaissance = @DateNaissance,
												     Telephone = @Telephone, Email = @Email, CodeOrgane = @CodeOrgane
												 WHERE Matricule = @Matricule + '00'";
		// L'identité d'un agent qui a un compte Locate suit celle du répertoire
		public static string UpdateCompte { get; } = @"UPDATE _Utilisateur
													   SET Nom = @Nom, Postnom = @Postnom, Prenom = @Prenom, Sexe = @SexeLettre, Telephone = @Telephone, Email = @Email
													   WHERE UserName = @Matricule";

		public static string SelectBySerialId { get; } = @"SELECT SerialId,Matricule,Nom,Postnom,Prenom,Sexe,DateNaissance,Telephone,Email,CodeOrgane
													FROM Agent
													WHERE SerialId = @Id";

		// Agents sans compte utilisateur (le matricule d'agent est le nom d'utilisateur suivi de « 00 »).
		public static string SelectAllActifNotUser { get; } = @"SELECT SerialId,Matricule,Nom,Postnom,Prenom,Sexe,DateNaissance,Telephone,Email,CodeOrgane
													FROM Agent
													WHERE NOT EXISTS (SELECT 1 FROM _Utilisateur WHERE _Utilisateur.UserName + '00' = RTRIM(Agent.Matricule))
													ORDER BY Nom, Postnom, Prenom";

		public static string SelectAllActifNotUserById { get; } = @"SELECT SerialId,Matricule,Nom,Postnom,Prenom,Sexe,DateNaissance,Telephone,Email,CodeOrgane
													FROM Agent
													WHERE NOT EXISTS (SELECT 1 FROM _Utilisateur WHERE _Utilisateur.UserName + '00' = RTRIM(Agent.Matricule))
													  AND (Matricule LIKE @Matricule OR Nom LIKE @Matricule OR Postnom LIKE @Matricule OR Prenom LIKE @Matricule)
													ORDER BY Nom, Postnom, Prenom";

	}
	
}