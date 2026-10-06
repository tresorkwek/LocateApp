using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlDashboard
    {
        public static string SelectById { get; } = @"SELECT IdAction,Nom
                                                    FROM _Action
                                                    WHERE IdAction = @IdAction";
    }
}