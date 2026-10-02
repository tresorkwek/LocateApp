using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlModule
    {
        public static string SelectAll { get; } = @"SELECT IdModule,Nom
                                                    FROM _Module";
        public static string SelectById { get; } = @"SELECT IdModule,Nom
                                                     FROM _Module
                                                     WHERE IdModule = @IdModule";
    }
}