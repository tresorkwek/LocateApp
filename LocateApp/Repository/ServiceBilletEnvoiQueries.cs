using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public class SqlServiceBilletEnvoi
    {
        public static string SelectAll { get; } = @"SELECT A.RefServiceHopital IdService,A.RefNumBilletEnvoi IdBilletEnvoi, CASE WHEN B.Denomination IS NULL THEN C.Denomination ELSE B.Denomination END Denomination,
	                                                                 A.DateCreation,A.UserCreation,A.IsApproved,A.ApprovedBy,A.IsUrgent,A.Observation,A.DateExpiration
                                                                FROM BilletEnvoiSpecialitesHop A 
		                                                                INNER JOIN ServicesHopital B ON A.RefServiceHopital = B.SerialId
		                                                                INNER JOIN ServiceMedical C ON B.RefServiceMed = C.IdService
                                                                WHERE A.RefNumBilletEnvoi = @IdBilletEnvoi
                                                                        AND  CAST(GETDATE() AS Date) <= DateExpiration
                                                                ORDER BY B.RefHopitalConv, C.IdService";

        public static string SelectUsedByService { get; } = @"SELECT IdPassage,Matricule,DatePassage,IdHopital,IdServiceHopital,IdBilletEnvoi,A.UserCreation,A.IsUrgent,A.Observation
                                                              FROM PassagePatientHopital A INNER JOIN BilletEnvoiSpecialitesHop B ON A.IdBilletEnvoi = B.RefNumBilletEnvoi AND 
                                                                   A.IdServiceHopital = B.RefServiceHopital
                                                              WHERE IdBilletEnvoi = @IdBilletEnvoi 
                                                                    AND IdServiceHopital = @IdServiceHopital
                                                                    AND  CAST(GETDATE() AS Date) <= DateExpiration
                                                                    AND CAST(GETDATE() AS Date) = CAST(DatePassage AS Date)";
    }
}