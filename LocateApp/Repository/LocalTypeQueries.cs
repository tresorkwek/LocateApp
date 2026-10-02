using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlLocalType
    {
        public static string SelectAll { get; } = @"SELECT Id,Nom
                                                    FROM LocalType";
        public static string SelectById { get; } = @"SELECT Id,Nom
                                                    FROM LocalType
                                                    WHERE Id = @Id";
    }
}