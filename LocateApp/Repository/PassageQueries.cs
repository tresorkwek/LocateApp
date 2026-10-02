using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlPassage
    {
        public static string SelectAll { get; } = @"SELECT IdBilletEnvoi,IdServiceHopital,UserCreation,DatePassage,IsUrgent,Observation
                                                    FROM PassagePatientHopital
                                                    WHERE YEAR(DatePassage) = YEAR(GETDATE()) AND MONTH(DatePassage) = MONTH(GETDATE())
                                                    ORDER BY DatePassage DESC";
        public static string SelectByPatient { get; } = @"SELECT IdBilletEnvoi,IdServiceHopital,UserCreation,DatePassage,IsUrgent,Observation
                                                          FROM PassagePatientHopital INNER JOIN Patients ON PassagePatientHopital.Matricule = Patients.Matricule
                                                          WHERE (Patients.Nom LIKE @Matricule OR Patients.Postnom LIKE @Matricule OR Patients.Prenom LIKE @Matricule OR Patients.Matricule LIKE @Matricule)
                                                                AND YEAR(DatePassage) = YEAR(GETDATE()) AND MONTH(DatePassage) = MONTH(GETDATE())
                                                          ORDER BY DatePassage DESC";

        public static string SelectByPatientAndHospital { get; } = @"SELECT IdBilletEnvoi,IdServiceHopital,UserCreation,DatePassage,IsUrgent,Observation
                                                                     FROM PassagePatientHopital INNER JOIN Patients ON PassagePatientHopital.Matricule = Patients.Matricule
                                                                     WHERE (Patients.Nom LIKE @Matricule OR Patients.Postnom LIKE @Matricule OR Patients.Prenom LIKE @Matricule OR Patients.Matricule LIKE @Matricule)
                                                                           AND IdHopital = @IdHopital AND YEAR(DatePassage) = YEAR(GETDATE()) AND MONTH(DatePassage) = MONTH(GETDATE())
                                                                     ORDER BY DatePassage DESC";

        public static string SelectByHospital { get; } = @"SELECT IdBilletEnvoi,IdServiceHopital,UserCreation,DatePassage,IsUrgent,Observation
                                                           FROM PassagePatientHopital
                                                           WHERE IdHopital =  @IdHopital AND YEAR(DatePassage) = YEAR(GETDATE()) AND MONTH(DatePassage) = MONTH(GETDATE())
                                                           ORDER BY DatePassage DESC";
    }
}