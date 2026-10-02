using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlSelectCategoriePatient
    {
        public static string All { get; } = @"SELECT IdCategoriePatient As Id,Nom
                                              FROM CategoriePatient";
        public static string Id { get; } = @"SELECT IdCategoriePatient As Id,Nom
                                             FROM CategoriePatient
                                             WHERE IdCategoriePatient = @Id";
    }
}