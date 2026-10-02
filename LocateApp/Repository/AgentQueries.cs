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

		public static string SelectAllActifNotUser { get; } = @"SELECT V_Patients.SerialId,V_Patients.Matricule,V_Patients.Nom,V_Patients.Postnom,V_Patients.Prenom,V_Patients.Sexe,V_Patients.Adresse,
														   format(V_Patients.DateNaissance,'dd MMM yyyy') AS DateNaissance,V_Patients.EtatCivil AS SituationFamiliale,Actif,V_Patients.Telephone,V_Patients.Email,
														   V_Patients.DateNaissance AS DateNaissanceBrute,PrisEnCharge,CodeStructureOrgane as CodeDirection,CodeStructureOrganeSD AS CodeSousDirection, CodeStructOrgService AS CodeService,
														   NomStructOrgane as Direction,DateFinValidite,Patient.Matricule AS MoviaMatricule,Patient.Nom AS MoviaNom,
														   Patient.Postnom AS MoviaPostnom,Patient.Prenom AS MoviaPrenom,Patient.Sexe AS MoviaSexe,
														   Patient.DateNaissance as MoviaDateNaissanceBrute,format(Patient.DateNaissance,'dd MMM yyyy') as MoviaDateNaissance,
														   Patient.EtatCivil as MoviaSituationFamiliale,accord as MoviaAccordAgent,commentaire as MoviaCommentaire,
														   AccordDRHPar as MoviaUserAccordDRH,
														   (SELECT CONCAT(Nom,' ',Postnom,' ',Prenom) FROM Patients
															where Patients.MATRICULE = CONCAT(AccordDRHPar,'00')) AS MoviaNomUserAccordDRH,
														   format(DateAccordDRH,'dd MMM yyyy') as MoviaDateAccordDRH,AccordAdminPar as MoviaUserAdminAccord,
														   (SELECT CONCAT(Nom,' ',Postnom,' ',Prenom)
    														FROM Patients 
															where Patients.MATRICULE = CONCAT(AccordAdminPar,'00')) AS MoviaNomUserAdminAccord,
														   format(DateAccordAdmin,'dd MMM yyyy') as MoviaDateAdminAccord,[User] as MoviaUserImpression, 
														   format(DateImpression,'dd MMM yyyy') as MoviaDateImpression,retirer as MoviaRetirer,
														   format(DateRetrait,'dd MMM yyyy') as MoviaDateRetrait,CodeHopitalPreference,IdCategoriePatient
	        
													FROM V_Patients LEFT JOIN Patient  ON V_Patients.Matricule = Patient.Matricule
																	LEFT JOIN impression ON patient.matricule = impression.agent

													WHERE V_Patients.Matricule Like '%00' AND V_Patients.Actif = 1";

		public static string SelectAllActifNotUserById { get; } = @"SELECT V_Patients.SerialId,V_Patients.Matricule,V_Patients.Nom,V_Patients.Postnom,V_Patients.Prenom,V_Patients.Sexe,V_Patients.Adresse,
														   format(V_Patients.DateNaissance,'dd MMM yyyy') AS DateNaissance,V_Patients.EtatCivil AS SituationFamiliale,Actif,V_Patients.Telephone,V_Patients.Email,
														   V_Patients.DateNaissance AS DateNaissanceBrute,PrisEnCharge,CodeStructureOrgane as CodeDirection,CodeStructureOrganeSD AS CodeSousDirection, CodeStructOrgService AS CodeService,
														   NomStructOrgane as Direction,DateFinValidite,Patient.Matricule AS MoviaMatricule,Patient.Nom AS MoviaNom,
														   Patient.Postnom AS MoviaPostnom,Patient.Prenom AS MoviaPrenom,Patient.Sexe AS MoviaSexe,
														   Patient.DateNaissance as MoviaDateNaissanceBrute,format(Patient.DateNaissance,'dd MMM yyyy') as MoviaDateNaissance,
														   Patient.EtatCivil as MoviaSituationFamiliale,accord as MoviaAccordAgent,commentaire as MoviaCommentaire,
														   AccordDRHPar as MoviaUserAccordDRH,
														   (SELECT CONCAT(Nom,' ',Postnom,' ',Prenom) FROM Patients
															where Patients.MATRICULE = CONCAT(AccordDRHPar,'00')) AS MoviaNomUserAccordDRH,
														   format(DateAccordDRH,'dd MMM yyyy') as MoviaDateAccordDRH,AccordAdminPar as MoviaUserAdminAccord,
														   (SELECT CONCAT(Nom,' ',Postnom,' ',Prenom)
    														FROM Patients 
															where Patients.MATRICULE = CONCAT(AccordAdminPar,'00')) AS MoviaNomUserAdminAccord,
														   format(DateAccordAdmin,'dd MMM yyyy') as MoviaDateAdminAccord,[User] as MoviaUserImpression, 
														   format(DateImpression,'dd MMM yyyy') as MoviaDateImpression,retirer as MoviaRetirer,
														   format(DateRetrait,'dd MMM yyyy') as MoviaDateRetrait,CodeHopitalPreference,IdCategoriePatient
	        
													FROM V_Patients LEFT JOIN Patient  ON V_Patients.Matricule = Patient.Matricule
																	LEFT JOIN impression ON patient.matricule = impression.agent

													WHERE Actif = 1 AND  V_Patients.Matricule Like '%00' AND V_Patients.Actif = 1 
														  AND (V_Patients.Matricule LIKE @Matricule OR V_Patients.Nom LIKE @Matricule OR V_Patients.Postnom LIKE @Matricule OR V_Patients.Prenom LIKE @Matricule)";

	}
	
}