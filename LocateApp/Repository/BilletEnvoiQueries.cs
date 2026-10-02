using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlBilletEnvoi 
    {
        public static string SelectAll { get; } = @"SELECT SerialId Id, NumBilletEnvoi, RefPatient Matricule, RefCodeHopital IdHopital,
		                                                RefCodeOrgane CodeOrgane, RefNomOrgane Organe,  UserCreation,
		                                                DateCreation,  DateExpiration, UserAnnulation, DateAnnulation,
		                                                IsCanceled,PrintNumber,Observation,IdBilletEnvoi
                                                FROM BilletEnvoi
                                                WHERE IsCanceled = 0";
        public static string SelectAllCanceled { get; } = @"SELECT SerialId Id, NumBilletEnvoi, RefPatient Matricule, RefCodeHopital IdHopital,
		                                                RefCodeOrgane CodeOrgane, RefNomOrgane Organe,  UserCreation,
		                                                DateCreation,  DateExpiration, UserAnnulation, DateAnnulation,
		                                                IsCanceled,PrintNumber,Observation,IdBilletEnvoi
                                                FROM BilletEnvoi
                                                WHERE IsCanceled = 1";

        public static string SelectById { get; } = @"SELECT SerialId Id, NumBilletEnvoi, RefPatient Matricule, RefCodeHopital IdHopital,
		                                                RefCodeOrgane CodeOrgane, RefNomOrgane Organe,  UserCreation,
		                                                DateCreation,  DateExpiration, UserAnnulation, DateAnnulation,
		                                                IsCanceled,PrintNumber,Observation,IdBilletEnvoi
                                              FROM BilletEnvoi
                                              WHERE SerialId = @Id";

        public static string SelectByIdAndInstitution { get; } = @"SELECT SerialId Id, NumBilletEnvoi, RefPatient Matricule, RefCodeHopital IdHopital,
		                                                RefCodeOrgane CodeOrgane, RefNomOrgane Organe,  UserCreation,
		                                                DateCreation, DateExpiration, UserAnnulation, DateAnnulation,
		                                                IsCanceled,PrintNumber,Observation,IdBilletEnvoi
                                              FROM BilletEnvoi
											  WHERE SerialId = @Id AND RefCodeHopital = @IdInstitution";

        public static string AllCurrent { get; } = @"SELECT DISTINCT A.SerialId Id, NumBilletEnvoi, RefPatient Matricule, RefCodeHopital IdHopital,
		                                                RefCodeOrgane CodeOrgane, RefNomOrgane Organe, A.UserCreation,
		                                                A.DateCreation,  A.DateExpiration, UserAnnulation, DateAnnulation,
		                                                IsCanceled,PrintNumber,A.Observation,IdBilletEnvoi
                                              FROM BilletEnvoi A INNER JOIN BilletEnvoiSpecialitesHop B ON A.SerialId = B.RefNumBilletEnvoi
                                              WHERE IsCanceled = 0 AND YEAR(A.DateCreation) = YEAR(GETDATE()) AND MONTH(A.DateCreation) = MONTH(GETDATE())";
        public static string Current { get; } = @"SELECT DISTINCT A.SerialId Id, NumBilletEnvoi, RefPatient Matricule, RefCodeHopital IdHopital,
		                                                RefCodeOrgane CodeOrgane, RefNomOrgane Organe, A.UserCreation,
		                                                A.DateCreation,  A.DateExpiration, UserAnnulation, DateAnnulation,
		                                                IsCanceled,PrintNumber,A.Observation,IdBilletEnvoi
                                              FROM BilletEnvoi A INNER JOIN BilletEnvoiSpecialitesHop B ON A.SerialId = B.RefNumBilletEnvoi
                                              WHERE B.IsApproved = 1 AND IsCanceled = 0 AND YEAR(A.DateCreation) = YEAR(GETDATE()) AND MONTH(A.DateCreation) = MONTH(GETDATE())";

        public static string CurrentNotApproved { get; } = @"SELECT DISTINCT A.SerialId Id, NumBilletEnvoi, RefPatient Matricule, RefCodeHopital IdHopital,
		                                                RefCodeOrgane CodeOrgane, RefNomOrgane Organe, A.UserCreation,
		                                                A.DateCreation,  A.DateExpiration, UserAnnulation, DateAnnulation,
		                                                IsCanceled,PrintNumber,A.Observation,IdBilletEnvoi
                                              FROM BilletEnvoi A INNER JOIN BilletEnvoiSpecialitesHop B ON A.SerialId = B.RefNumBilletEnvoi
                                              WHERE B.IsApproved = 0 AND IsCanceled = 0 AND YEAR(A.DateCreation) = YEAR(GETDATE()) AND MONTH(A.DateCreation) = MONTH(GETDATE())";

        public static string CurrentAllByNip { get; } = @"SELECT DISTINCT A.SerialId Id, NumBilletEnvoi, RefPatient Matricule, RefCodeHopital IdHopital,
		                                                RefCodeOrgane CodeOrgane, RefNomOrgane Organe, A.UserCreation,
		                                                A.DateCreation,  A.DateExpiration, UserAnnulation, DateAnnulation,
		                                                IsCanceled,PrintNumber,A.Observation,IdBilletEnvoi
                                              FROM BilletEnvoi A INNER JOIN BilletEnvoiSpecialitesHop B ON A.SerialId = B.RefNumBilletEnvoi
                                              WHERE IsCanceled = 0 AND RefPatient like @Nip AND YEAR(A.DateCreation) = YEAR(GETDATE()) AND MONTH(A.DateCreation) = MONTH(GETDATE())";

        public static string CurrentByNip { get; } = @"SELECT DISTINCT A.SerialId Id, NumBilletEnvoi, RefPatient Matricule, RefCodeHopital IdHopital,
		                                                RefCodeOrgane CodeOrgane, RefNomOrgane Organe, A.UserCreation,
		                                                A.DateCreation,  A.DateExpiration, UserAnnulation, DateAnnulation,
		                                                IsCanceled,PrintNumber,A.Observation,IdBilletEnvoi
                                              FROM BilletEnvoi A INNER JOIN BilletEnvoiSpecialitesHop B ON A.SerialId = B.RefNumBilletEnvoi
                                              WHERE B.IsApproved = 1 AND IsCanceled = 0 AND RefPatient like @Nip AND YEAR(A.DateCreation) = YEAR(GETDATE()) AND MONTH(A.DateCreation) = MONTH(GETDATE())";

        public static string CurrentByNipNotApproved { get; } = @"SELECT DISTINCT A.SerialId Id, NumBilletEnvoi, RefPatient Matricule, RefCodeHopital IdHopital,
		                                                RefCodeOrgane CodeOrgane, RefNomOrgane Organe, A.UserCreation,
		                                                A.DateCreation,  A.DateExpiration, UserAnnulation, DateAnnulation,
		                                                IsCanceled,PrintNumber,A.Observation,IdBilletEnvoi
                                              FROM BilletEnvoi A INNER JOIN BilletEnvoiSpecialitesHop B ON A.SerialId = B.RefNumBilletEnvoi
                                              WHERE B.IsApproved = 0 AND RefPatient like @Nip AND IsCanceled = 0 AND YEAR(A.DateCreation) = YEAR(GETDATE()) AND MONTH(A.DateCreation) = MONTH(GETDATE())";

        public static string CurrentByHospital { get; } = @"SELECT DISTINCT A.SerialId Id, NumBilletEnvoi, RefPatient Matricule, RefCodeHopital IdHopital,
		                                                RefCodeOrgane CodeOrgane, RefNomOrgane Organe, A.UserCreation,
		                                                A.DateCreation,  A.DateExpiration, UserAnnulation, DateAnnulation,
		                                                IsCanceled,PrintNumber,A.Observation,IdBilletEnvoi
                                              FROM BilletEnvoi A INNER JOIN BilletEnvoiSpecialitesHop B ON A.SerialId = B.RefNumBilletEnvoi
                                              WHERE B.IsApproved = 1 AND IsCanceled = 0 AND RefCodeHopital = @Id AND YEAR(A.DateCreation) = YEAR(GETDATE()) AND MONTH(A.DateCreation) = MONTH(GETDATE())";
        public static string CurrentByHospitalNotApproved { get; } = @"SELECT DISTINCT A.SerialId Id, NumBilletEnvoi, RefPatient Matricule, RefCodeHopital IdHopital,
		                                                RefCodeOrgane CodeOrgane, RefNomOrgane Organe, A.UserCreation,
		                                                A.DateCreation,  A.DateExpiration, UserAnnulation, DateAnnulation,
		                                                IsCanceled,PrintNumber,A.Observation,IdBilletEnvoi
                                              FROM BilletEnvoi A INNER JOIN BilletEnvoiSpecialitesHop B ON A.SerialId = B.RefNumBilletEnvoi
                                              WHERE B.IsApproved = 0 AND IsCanceled = 0 AND RefCodeHopital = @Id AND YEAR(A.DateCreation) = YEAR(GETDATE()) AND MONTH(A.DateCreation) = MONTH(GETDATE())";

        public static string CurrentByHospitalAndNip { get; } = @"SELECT DISTINCT A.SerialId Id, NumBilletEnvoi, RefPatient Matricule, RefCodeHopital IdHopital,
		                                                RefCodeOrgane CodeOrgane, RefNomOrgane Organe, A.UserCreation,
		                                                A.DateCreation,  A.DateExpiration, UserAnnulation, DateAnnulation,
		                                                IsCanceled,PrintNumber,A.Observation,IdBilletEnvoi
                                              FROM BilletEnvoi A INNER JOIN BilletEnvoiSpecialitesHop B ON A.SerialId = B.RefNumBilletEnvoi
                                              WHERE B.IsApproved = 1 AND IsCanceled = 0 AND RefCodeHopital = @Id AND RefPatient like @Nip AND YEAR(A.DateCreation) = YEAR(GETDATE()) AND MONTH(A.DateCreation) = MONTH(GETDATE())";

        public static string CurrentByHospitalAndNipNotApproved { get; } = @"SELECT DISTINCT A.SerialId Id, NumBilletEnvoi, RefPatient Matricule, RefCodeHopital IdHopital,
		                                                RefCodeOrgane CodeOrgane, RefNomOrgane Organe, A.UserCreation,
		                                                A.DateCreation,  A.DateExpiration, UserAnnulation, DateAnnulation,
		                                                IsCanceled,PrintNumber,A.Observation,IdBilletEnvoi
                                              FROM BilletEnvoi A INNER JOIN BilletEnvoiSpecialitesHop B ON A.SerialId = B.RefNumBilletEnvoi
                                              WHERE B.IsApproved = 0 AND IsCanceled = 0 AND RefCodeHopital = @Id AND RefPatient like @Nip AND YEAR(A.DateCreation) = YEAR(GETDATE()) AND MONTH(A.DateCreation) = MONTH(GETDATE())";

        public static string Approve { get; } = @"UPDATE BilletEnvoiSpecialitesHop
                                                  SET IsApproved = 1, ApprovedBy = @UserName, ApprovedAt = GETDATE()
                                                  WHERE RefNumBilletEnvoi = @IdBilletEnvoi AND RefServiceHopital = @IdService";
        public static string UpdatePrintNumber { get; } = @"UPDATE BilletEnvoi
                                                         SET PrintNumber += 1
                                                         WHERE SerialId = @Id";
        public static string UpdatePrintNumberdAndInstitution { get; } = @"UPDATE BilletEnvoi
                                                                           SET PrintNumber += 1
											                               WHERE SerialId = @Id AND RefCodeHopital = @IdInstitution";
        public static string Cancel { get; } = @"UPDATE BilletEnvoi
                                                 SET IsCanceled = 1, UserAnnulation = @UserName, DateAnnulation = @DateAnnulation
											     WHERE SerialId = @Id";

        //public static string Add { get; } = @"spBilletEnvoi_Insert";
        public static string CheckAndAdd { get; } = @"spBilletEnvoi_CheckAndInsert";
        //public static string Modify { get; } = @"spBilletEnvoi_Update";
        public static string CheckAndModify { get; } = @"spBilletEnvoi_CheckAndUpdate";
        public static string Delete { get; } = @"spBilletEnvoi_Delete";
    }
}