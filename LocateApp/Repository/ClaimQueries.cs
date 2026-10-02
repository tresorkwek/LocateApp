using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlClaim
    {

        public static string SelectAll { get; } = @"SELECT _Menu.Nom
                                                    FROM _Menu";
        public static string SelectAllForConfig { get; } = @"SELECT IdProfil,IdMenu,CustomUrl
                                                             FROM _Claim";
        public static string SelectByIdForConfig { get; } = @"SELECT IdProfil,IdMenu,CustomUrl
                                                             FROM _Claim
                                                             WHERE IdClaim = @IdClaim";
        public static string SelectByProfilForConfig { get; } = @"SELECT IdProfil,IdMenu,CustomUrl
                                                             FROM _Claim
                                                             WHERE IdProfil = @IdProfil";

        public static string SelectByName { get; } = @"SELECT _Menu.Nom
                                                     FROM _Menu 
                                                     WHERE Nom = @Claim";
        public static string SelectByProfil { get; } = @"SELECT _Menu.Nom
                                                         FROM _Menu  INNER JOIN _Claim  ON _Menu.IdMenu = _Claim.IdMenu
                                                         WHERE _Claim.IdProfil = @IdProfil";
        public static string SelectForAgent { get; } = @"SELECT _Menu.Nom
                                                         FROM _Menu 
                                                         WHERE _Menu.DefaultMenu = 1";

        public static string SelectForAgentByProfil { get; } = @"SELECT _Menu.Nom
                                                                 FROM _Menu 
                                                                 WHERE _Menu.DefaultMenu = 1

                                                                 UNION 

                                                                 SELECT _Menu.Nom
                                                                 FROM _Menu  INNER JOIN _Claim  ON _Menu.IdMenu = _Claim.IdMenu
                                                                 WHERE _Claim.IdProfil = @IdProfil";

        public static string Insert { get; } = @"INSERT INTO _Claim(IdProfil,IdMenu)
                                                 VALUES (@IdProfil,@IdMenu)";

        public static string Delete { get; } = @"DELETE FROM _Claim
                                                 WHERE IdProfil = @IdProfil";
    }
}