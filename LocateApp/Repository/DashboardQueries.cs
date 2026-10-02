using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlDashboard
    {
        public static string SelectPriseEnChargeByDate { get; } = @"SELECT CAST (datepassage AS date) [DatePassage],COUNT(*) as Nbre FROM PassagePatientHopital
                                                                   WHERE MONTH(datepassage) = MONTH(GETDATE()) AND YEAR(datepassage) = YEAR(GETDATE())
                                                                   GROUP BY CAST (datepassage AS date)";
        public static string SelectById { get; } = @"SELECT IdAction,Nom
                                                    FROM _Action
                                                    WHERE IdAction = @IdAction";
    }
}