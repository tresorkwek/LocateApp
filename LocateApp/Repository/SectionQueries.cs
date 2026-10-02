using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlSection
    {
        public static string SelectAll { get; } = @"SELECT IdSection,Nom
                                                    FROM _Section";
        public static string SelectById { get; } = @"SELECT IdSection,Nom
                                                    FROM _Section
                                                     WHERE IdSection = @IdSection";
    }
}