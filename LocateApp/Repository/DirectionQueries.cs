using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlDirection
    {

        public static string SelectAll { get; } = @"SELECT Code,Libelle as Nom
                                              FROM [Clinique].[dbo].T_Directions_BCC
                                              ORDER BY Code";
        public static string SelectById { get; } = @"SELECT Code,Libelle as Nom
                                             FROM [Clinique].[dbo].T_Directions_BCC
                                             WHERE Code = @Code";
        public static string SelectByAgent { get; } = @"SELECT  b.Code , Libelle as Nom
												FROM [clinique].[dbo].[v_patient_direction] a JOIN [clinique].[dbo].[T_Directions_BCC] b
																	ON a.valeur=b.Code
												WHERE MATRICULE = @Matricule";
    }
}