using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlPatient
	{				
		public static string SelectAll { get; } = @"SELECT V_Patients.SerialId,V_Patients.Matricule,V_Patients.Nom,V_Patients.Postnom,V_Patients.Prenom,V_Patients.Sexe,V_Patients.Adresse,
														   format(V_Patients.DateNaissance,'dd MMM yyyy') AS DateNaissance,V_Patients.EtatCivil AS SituationFamiliale,Actif,V_Patients.Telephone,V_Patients.Email,
														   V_Patients.DateNaissance AS DateNaissanceBrute,PrisEnCharge,CodeStructureOrgane as CodeDirection,
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

													WHERE V_Patients.Matricule is not null AND PrisEnCharge = 'OUI'";
		public static string SelectById { get; } = @"SELECT V_Patients.SerialId,V_Patients.Matricule,V_Patients.Nom,V_Patients.Postnom,V_Patients.Prenom,V_Patients.Sexe,V_Patients.Adresse,
														   format(V_Patients.DateNaissance,'dd MMM yyyy') AS DateNaissance,V_Patients.EtatCivil AS SituationFamiliale,Actif,V_Patients.Telephone,V_Patients.Email,
														   V_Patients.DateNaissance AS DateNaissanceBrute,PrisEnCharge,CodeStructureOrgane as CodeDirection,
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

													WHERE V_Patients.Matricule is not null AND V_Patients.Matricule LIKE @Matricule";

		public static string SelectPatientBySerialId { get; } = @"SELECT V_Patients.SerialId,V_Patients.Matricule,V_Patients.Nom,V_Patients.Postnom,V_Patients.Prenom,V_Patients.Sexe,V_Patients.Adresse,
																	   format(V_Patients.DateNaissance,'dd MMM yyyy') AS DateNaissance,V_Patients.EtatCivil AS SituationFamiliale,Actif,V_Patients.Telephone,V_Patients.Email,
																	   V_Patients.DateNaissance AS DateNaissanceBrute,PrisEnCharge,CodeStructureOrgane as CodeDirection,
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

																WHERE V_Patients.SerialId = @Id";


		public static string SelectByName { get; } = @"SELECT V_Patients.SerialId,V_Patients.Matricule,V_Patients.Nom,V_Patients.Postnom,V_Patients.Prenom,V_Patients.Sexe,V_Patients.Adresse,
														   format(V_Patients.DateNaissance,'dd MMM yyyy') AS DateNaissance,V_Patients.EtatCivil AS SituationFamiliale,Actif,V_Patients.Telephone,V_Patients.Email,
														   V_Patients.DateNaissance AS DateNaissanceBrute,PrisEnCharge,CodeStructureOrgane as CodeDirection,
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

													WHERE V_Patients.Matricule is not null AND 
														  (V_Patients.Nom LIKE @Nom OR V_Patients.Postnom LIKE @Nom OR V_Patients.Prenom LIKE @Nom OR V_Patients.Matricule LIKE @Nom)";
				
		public static string SelectCorrection { get; } = @"SELECT V_Patients.SerialId,V_Patients.Matricule,V_Patients.Nom,V_Patients.Postnom,V_Patients.Prenom,V_Patients.Sexe,
																   V_Patients.Adresse,format(V_Patients.DateNaissance,'dd MMM yyyy') AS DateNaissance,
																   V_Patients.EtatCivil AS SituationFamiliale,Actif,V_Patients.Telephone,V_Patients.Email,
																   V_Patients.DateNaissance AS DateNaissanceBrute,PrisEnCharge,CodeStructureOrgane as CodeDirection,
																   NomStructOrgane as Direction,DateFinValidite,Patient.Matricule AS MoviaMatricule,Patient.Nom AS MoviaNom,
																   Patient.Postnom AS MoviaPostnom,Patient.Prenom AS MoviaPrenom,Patient.Sexe AS MoviaSexe,
																   Patient.DateNaissance as MoviaDateNaissanceBrute,format(Patient.DateNaissance,'dd MMM yyyy') as MoviaDateNaissance,
																   Patient.EtatCivil as MoviaSituationFamiliale,accord as MoviaAccordAgent,commentaire as MoviaCommentaire,
																   AccordDRHPar as MoviaUserAccordDRH,
																   (SELECT CONCAT(Nom,' ',Postnom,' ',Prenom) FROM Patients
																	where Patients.MATRICULE = CONCAT(AccordDRHPar,'00')) AS MoviaNomUserAccordDRH,
																   format(DateAccordDRH,'dd MMM yyyy') as MoviaDateAccordDRH,AccordAdminPar as MoviaUserAdminAccord,
																   (SELECT CONCAT(Nom,' ',Postnom,' ',Prenom) FROM Patients 
																	where Patients.MATRICULE = CONCAT(AccordAdminPar,'00')) AS MoviaNomUserAdminAccord,
																   format(DateAccordAdmin,'dd MMM yyyy') as MoviaDateAdminAccord,[User] as MoviaUserImpression, 
																   format(DateImpression,'dd MMM yyyy') as MoviaDateImpression,retirer as MoviaRetirer,
																   format(DateRetrait,'dd MMM yyyy') as MoviaDateRetrait,CodeHopitalPreference,IdCategoriePatient
	        
															FROM V_Patients LEFT JOIN Patient  ON V_Patients.Matricule = Patient.Matricule
	   																		LEFT JOIN impression ON patient.matricule = impression.agent

															WHERE V_Patients.Matricule is not null AND PrisEnCharge = 'OUI' AND (Patient.Accord is null OR Patient.Accord = 0 ) 
																  AND (Patient.AccordDRHPar is null OR Patient.AccordDRHPar = 0)
																  AND (Patient.Matricule IS NOT NULL OR Patient.Nom IS NOT NULL OR Patient.Postnom IS NOT NULL OR Patient.Prenom IS NOT NULL OR Patient.Sexe IS NOT NULL OR Patient.DateNaissance IS NOT NULL OR Patient.EtatCivil IS NOT NULL)
																  AND ( (LEN(RTRIM(LTRIM(Patient.Commentaire))) != 0  AND Patient.AccordAdminPar is not null) 
																		OR Patient.Commentaire IS NULL OR LEN(RTRIM(LTRIM(Patient.Commentaire))) = 0) ";

		public static string SelectCorrectionById { get; } = @"SELECT V_Patients.SerialId,V_Patients.Matricule,V_Patients.Nom,V_Patients.Postnom,V_Patients.Prenom,V_Patients.Sexe,
																   V_Patients.Adresse,format(V_Patients.DateNaissance,'dd MMM yyyy') AS DateNaissance,
																   V_Patients.EtatCivil AS SituationFamiliale,Actif,V_Patients.Telephone,V_Patients.Email,
																   V_Patients.DateNaissance AS DateNaissanceBrute,PrisEnCharge,CodeStructureOrgane as CodeDirection,
																   NomStructOrgane as Direction,DateFinValidite,Patient.Matricule AS MoviaMatricule,Patient.Nom AS MoviaNom,
																   Patient.Postnom AS MoviaPostnom,Patient.Prenom AS MoviaPrenom,Patient.Sexe AS MoviaSexe,
																   Patient.DateNaissance as MoviaDateNaissanceBrute,format(Patient.DateNaissance,'dd MMM yyyy') as MoviaDateNaissance,
																   Patient.EtatCivil as MoviaSituationFamiliale,accord as MoviaAccordAgent,commentaire as MoviaCommentaire,
																   AccordDRHPar as MoviaUserAccordDRH,
																   (SELECT CONCAT(Nom,' ',Postnom,' ',Prenom) FROM Patients
																	where Patients.MATRICULE = CONCAT(AccordDRHPar,'00')) AS MoviaNomUserAccordDRH,
																   format(DateAccordDRH,'dd MMM yyyy') as MoviaDateAccordDRH,AccordAdminPar as MoviaUserAdminAccord,
																   (SELECT CONCAT(Nom,' ',Postnom,' ',Prenom) FROM Patients 
																	where Patients.MATRICULE = CONCAT(AccordAdminPar,'00')) AS MoviaNomUserAdminAccord,
																   format(DateAccordAdmin,'dd MMM yyyy') as MoviaDateAdminAccord,[User] as MoviaUserImpression, 
																   format(DateImpression,'dd MMM yyyy') as MoviaDateImpression,retirer as MoviaRetirer,
																   format(DateRetrait,'dd MMM yyyy') as MoviaDateRetrait,CodeHopitalPreference,IdCategoriePatient
	        
															FROM V_Patients LEFT JOIN Patient  ON V_Patients.Matricule = Patient.Matricule
	   																		LEFT JOIN impression ON patient.matricule = impression.agent

															WHERE V_Patients.Matricule is not null AND PrisEnCharge = 'OUI' AND (Patient.Accord is null OR Patient.Accord = 0 ) 
																  AND (Patient.AccordDRHPar is null OR Patient.AccordDRHPar = 0)
																  AND (Patient.Matricule IS NOT NULL OR Patient.Nom IS NOT NULL OR Patient.Postnom IS NOT NULL OR Patient.Prenom IS NOT NULL OR Patient.Sexe IS NOT NULL OR Patient.DateNaissance IS NOT NULL OR Patient.EtatCivil IS NOT NULL)
																  AND ( (LEN(RTRIM(LTRIM(Patient.Commentaire))) != 0  AND Patient.AccordAdminPar is not null) 
																		OR Patient.Commentaire IS NULL OR LEN(RTRIM(LTRIM(Patient.Commentaire))) = 0) 
																  AND (V_Patients.Nom LIKE @Matricule OR V_Patients.Postnom LIKE @Matricule OR V_Patients.Prenom LIKE @Matricule OR V_Patients.Matricule LIKE @Matricule)";

		public static string SelectRepair { get; } = @"SELECT V_Patients.SerialId,V_Patients.Matricule,V_Patients.Nom,V_Patients.Postnom,V_Patients.Prenom,V_Patients.Sexe,V_Patients.Adresse,
																   format(V_Patients.DateNaissance,'dd MMM yyyy') AS DateNaissance,V_Patients.EtatCivil AS SituationFamiliale,Actif,V_Patients.Telephone,V_Patients.Email,
																   V_Patients.DateNaissance AS DateNaissanceBrute,PrisEnCharge,CodeStructureOrgane as CodeDirection,
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
	        
															FROM V_Patients INNER JOIN Patient  ON V_Patients.Matricule = Patient.Matricule
																			LEFT JOIN impression ON patient.matricule = impression.agent

															WHERE V_Patients.Matricule is not null  AND patient.AccordDRHPar is null AND patient.Accord = '0'
																  AND  LEN(RTRIM(LTRIM(Patient.Commentaire))) != 0  AND Patient.AccordAdminPar is null ";

		public static string Delete { get; } = @"DELETE FROM Patients  
												 WHERE Matricule = @Matricule";

	}

	public static class SqlPatientMovia
	{

		public static string SelectAll { get; } = @"SELECT Matricule,Nom,Postnom,Prenom,Sexe,format(DateNaissance,'dd MMM yyyy') as DateNaissance
														  ,EtatCivil as SituationFamiliale,accord as AccordAgent,commentaire as Commentaire,AccordDRHPar as UserAccordDRH
														  ,(SELECT CONCAT(Nom,' ',Postnom,' ',Prenom)
     														FROM Patients
															where Patients.MATRICULE = CONCAT(AccordDRHPar,'00')) AS NomUserAccordDRH
														  ,format(DateAccordDRH,'dd MMM yyyy') as DateAccordDRH,AccordAdminPar as UserAdminAccord
														  ,(SELECT CONCAT(Nom,' ',Postnom,' ',Prenom)
    														FROM Patients 
															where Patients.MATRICULE = CONCAT(AccordAdminPar,'00')) AS NomUserAdminAccord
														  ,format(DateAccordAdmin,'dd MMM yyyy') as DateAdminAccord
														  ,[User] as UserImpression, format(DateImpression,'dd MMM yyyy') as DateImpression,retirer as Retirer,
														  format(DateRetrait,'dd MMM yyyy') as DateRetrait

											FROM patient LEFT JOIN impression ON patient.matricule = impression.agent";

		public static string SelectById { get; } = @"SELECT Matricule,Nom,Postnom,Prenom,Sexe,format(DateNaissance,'dd MMM yyyy') as DateNaissance
														  ,EtatCivil as SituationFamiliale,accord as AccordAgent,commentaire as Commentaire,AccordDRHPar as UserAccordDRH
														  ,(SELECT CONCAT(Nom,' ',Postnom,' ',Prenom)
     														FROM Patients
															where Patients.MATRICULE = CONCAT(AccordDRHPar,'00')) AS NomUserAccordDRH
														  ,format(DateAccordDRH,'dd MMM yyyy') as DateAccordDRH,AccordAdminPar as UserAdminAccord
														  ,(SELECT CONCAT(Nom,' ',Postnom,' ',Prenom)
    														FROM Patients 
															where Patients.MATRICULE = CONCAT(AccordAdminPar,'00')) AS NomUserAdminAccord
														  ,format(DateAccordAdmin,'dd MMM yyyy') as DateAdminAccord
														  ,[User] as UserImpression, format(DateImpression,'dd MMM yyyy') as DateImpression,retirer as Retirer,
														  format(DateRetrait,'dd MMM yyyy') as DateRetrait

													FROM patient LEFT JOIN impression ON patient.matricule = impression.agent
													WHERE Matricule = @Matricule";
		public static string Insert { get; } = @"INSERT INTO Patient (Matricule,Nom,Postnom,Prenom,Sexe,Commentaire)  
												 VALUES (@Matricule,@Nom,@Postnom,@Prenom,@Sexe,@Commentaire)";
		public static string Update { get; } = @"UPDATE Patient 
												 SET Nom = @Nom,Postnom = @Postnom,Prenom = @Prenom,Sexe = @Sexe,Commentaire = @Commentaire  
												 WHERE Matricule = @Matricule";
		public static string Delete { get; } = @"DELETE FROM Patient  
												 WHERE Matricule = @Matricule";

		public static string InsertDaccord { get; } = @"INSERT INTO Patient(Matricule,Accord) 
													   VALUES (@Matricule,'1')";
        public static string UpdateDaccord { get; } = @"UPDATE Patient 
													   SET Accord='1'
													   WHERE Matricule = @Matricule";
		public static string Correct { get; } = @"UPDATE Patient 
												  SET AccordDRHPar = @UserAccordDRH, DateAccordDRH = SYSDATETIME() , Commentaire = NULL
												  WHERE Matricule = @Matricule";
		public static string Imprimer { get; } = @"INSERT INTO Impression([User],Agent) 
												   VALUES (@UserName, @Matricule)";
		public static string DeleteImpression { get; } = @"DELETE FROM Impression WHERE Agent = @Matricule";

		public static string TransfertToDRH { get; } = @"UPDATE Patient 
														 SET AccordAdminPar = @UserAccordAdmin, DateAccordAdmin = SYSDATETIME()
														 WHERE Matricule = @Matricule";

	}

	
}