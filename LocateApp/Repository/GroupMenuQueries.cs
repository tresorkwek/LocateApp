using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlGroupMenu
    {
        public static string SelectAll { get; } = @"SELECT IdGroupMenu,Nom,Libelle,Url,Icone,IdSection,Visible,DefaultInternalUser,DefaultExternalUser
                                                    FROM _GroupMenu
                                                    ORDER BY IdSection,Ordre";
        public static string SelectAllVisible { get; } = @"SELECT IdGroupMenu,Nom,Libelle,Url,Icone,IdSection,Visible,DefaultInternalUser,DefaultExternalUser
                                                    FROM _GroupMenu
                                                    WHERE Visible = 1
                                                    ORDER BY Ordre";
        public static string SelectById { get; } = @"SELECT IdGroupMenu,Nom,Libelle,Url,Icone,IdSection,Visible,DefaultInternalUser,DefaultExternalUser
                                                     FROM _GroupMenu
                                                     WHERE IdGroupMenu = @IdGroupMenu";

        public static string SelectAgent { get; } = @"SELECT _GroupMenu.IdGroupMenu,_GroupMenu.Nom,_GroupMenu.Libelle,_GroupMenu.Url,_GroupMenu.Icone,
	                                                            _GroupMenu.IdSection,_GroupMenu.Visible,DefaultInternalUser,DefaultExternalUser,_GroupMenu.Ordre  
                                                         FROM _GroupMenu
                                                         WHERE  _GroupMenu.DefaultInternalUser = 1 AND _GroupMenu.Visible = 1";
        public static string SelectAgentByProfil { get; } = @"SELECT _GroupMenu.IdGroupMenu,_GroupMenu.Nom,_GroupMenu.Libelle,_GroupMenu.Url,_GroupMenu.Icone,
	                                                            _GroupMenu.IdSection,_GroupMenu.Visible,DefaultInternalUser,DefaultExternalUser,_GroupMenu.Ordre  
                                                         FROM _GroupMenu
                                                         WHERE  _GroupMenu.DefaultInternalUser = 1 AND _GroupMenu.Visible = 1

                                                         UNION

                                                         SELECT _GroupMenu.IdGroupMenu,_GroupMenu.Nom,_GroupMenu.Libelle,_GroupMenu.Url,_GroupMenu.Icone,
	                                                            _GroupMenu.IdSection,_GroupMenu.Visible,DefaultInternalUser,DefaultExternalUser,_GroupMenu.Ordre   
                                                         FROM _GroupMenu INNER JOIN _Menu ON _GroupMenu.IdGroupMenu = _Menu.IdGroupMenu
                                                                         INNER JOIN _Claim ON _Menu.idMenu = _Claim.idMenu
                                                         WHERE _GroupMenu.Visible = 1 AND _Menu.Visible = 1 AND _Claim.IdProfil = @IdProfil
                                                         ORDER BY _GroupMenu.IdSection,_GroupMenu.Ordre";

        public static string SelectByProfil { get; } = @"SELECT _GroupMenu.IdGroupMenu,_GroupMenu.Nom,_GroupMenu.Libelle,_GroupMenu.Url,_GroupMenu.Icone,
	                                                            _GroupMenu.IdSection,_GroupMenu.Visible,DefaultInternalUser,DefaultExternalUser,_GroupMenu.Ordre  
                                                         FROM _GroupMenu
                                                         WHERE _GroupMenu.DefaultExternalUser = 1 AND _GroupMenu.Visible = 1

                                                         UNION

                                                         SELECT _GroupMenu.IdGroupMenu,_GroupMenu.Nom,_GroupMenu.Libelle,_GroupMenu.Url,_GroupMenu.Icone,
	                                                            _GroupMenu.IdSection,_GroupMenu.Visible,DefaultInternalUser,DefaultExternalUser,_GroupMenu.Ordre   
                                                         FROM _GroupMenu INNER JOIN _Menu ON _GroupMenu.IdGroupMenu = _Menu.IdGroupMenu
                                                                         INNER JOIN _Claim ON _Menu.idMenu = _Claim.idMenu
                                                         WHERE _GroupMenu.Visible = 1 AND _Menu.Visible = 1 AND _Claim.IdProfil = @IdProfil
                                                         ORDER BY _GroupMenu.IdSection,_GroupMenu.Ordre";

        public static string MakeVisible { get; } = @"UPDATE _GroupMenu 
                                                  SET Visible = 1
                                                  WHERE IdGroupMenu = @IdGroupMenu";
        public static string MakeInVisible { get; } = @"UPDATE _GroupMenu 
                                                    SET Visible = 0
                                                    WHERE IdGroupMenu = @IdGroupMenu";
    }
}