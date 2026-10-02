using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlObservations
    {
        public static string SelectAll { get; } = @"SELECT Id,Observation,Etat
                                                    FROM ImmoObservation";
        public static string SelectById { get; } = @"SELECT Id,Observation,Etat
                                                    FROM ImmoObservation
                                                    WHERE Id = @Id";
        public static string SelectByEtat { get; } = @"SELECT Id,Observation,Etat
                                                    FROM ImmoObservation
                                                    WHERE Etat = @Etat";
        public static string SelectStat { get; } = @"SELECT DISTINCT o.Observation,
														(SELECT COUNT(*) FROM Immo a INNER JOIN ImmoObservation b ON a.IdLastObservation = b.Id
														  Where b.Observation = o.Observation AND a.LastEtat = 'B' AND a.IsActive = 1 AND a.QrCode IS NOT NULL) AS NbreBon,

														  CONVERT(decimal(18,2),
		  
														  CONVERT(decimal(18,2), (SELECT COUNT(*) FROM Immo a INNER JOIN ImmoObservation b ON a.IdLastObservation = b.Id
														  Where b.Observation = o.Observation AND a.LastEtat = 'B' AND a.IsActive = 1 AND a.QrCode IS NOT NULL))/
		  
														  CONVERT(decimal(18,2), (SELECT COUNT(*) FROM Immo a
														  Where a.LastEtat = 'B' AND a.IsActive = 1 AND a.QrCode IS NOT NULL)) *100
		  
		  
														  ) as PourcentageBon,   

														  (SELECT COUNT(*) FROM Immo a INNER JOIN ImmoObservation b ON a.IdLastObservation = b.Id
															  Where b.Observation = o.Observation AND a.LastEtat = 'M' AND a.IsActive = 1 AND a.QrCode IS NOT NULL) AS NbreMauvais,

														  CONVERT(decimal(18,2),
		  
														 CONVERT(decimal(18,2), (SELECT COUNT(*) FROM Immo a INNER JOIN ImmoObservation b ON a.IdLastObservation = b.Id
														  Where b.Observation = o.Observation AND a.LastEtat = 'M' AND a.IsActive = 1 AND a.QrCode IS NOT NULL))/
		  
														  CONVERT(decimal(18,2), (SELECT COUNT(*) FROM Immo a
														  Where a.LastEtat = 'M' AND a.IsActive = 1 AND a.QrCode IS NOT NULL)) *100
		  
		  
														  ) as PourcentageMauvais
												FROM ImmoObservation o
												WHERE (SELECT COUNT(*) FROM Immo a INNER JOIN ImmoObservation b ON a.IdLastObservation = b.Id
														  Where b.Observation = o.Observation AND a.IsActive = 1)> 0
												ORDER BY NbreBon DESC, NbreMauvais DESC";
        public static string Insert { get; } = @"INSERT INTO ImmoObservation (Observation,Etat)
                                                 VALUES (@Observation,@Etat)";

        public static string Update { get; } = @"UPDATE ImmoObservation 
                                                 SET Observation = @Observation, Etat = @Etat
                                                 WHERE Id = @Id";

        public static string Delete { get; } = @"DELETE FROM ImmoObservation WHERE Id = @Id";
    }
}