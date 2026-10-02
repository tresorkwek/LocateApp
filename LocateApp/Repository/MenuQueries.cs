using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlMenu
    {
        public static string SelectAll { get; } = @"SELECT IdMenu,Nom,Libelle,Icone,Commentaire,Titre,Url,IdGroupMenu,Ordre,
	                                                       Visible,IdModule,IdAction,DefaultMenu
                                                    FROM _Menu
                                                    ORDER BY IdGroupMenu,IdModule,IdAction,Ordre";
        public static string SelectAllVisible { get; } = @"SELECT IdMenu,Nom,Libelle,Icone,Commentaire,Titre,Url,IdGroupMenu,Ordre,
	                                                       Visible,IdModule,IdAction,DefaultMenu
                                                    FROM _Menu
                                                    WHERE Visible = 1
                                                    ORDER BY IdGroupMenu,Ordre";

        public static string SelectById { get; } = @"SELECT IdMenu,Nom,Libelle,Icone,Commentaire,Titre,Url,IdGroupMenu,Ordre,
	                                                        Visible,IdModule,IdAction,DefaultMenu
                                                     FROM _Menu
                                                     WHERE IdMenu = @IdMenu";
        public static string SelectByName { get; } = @"SELECT IdMenu,Nom,Libelle,Icone,Commentaire,Titre,Url,IdGroupMenu,Ordre,
	                                                        Visible,IdModule,IdAction,DefaultMenu
                                                     FROM _Menu
                                                     WHERE Nom = @Nom";
        public static string SelectByGroupMenu { get; } = @"SELECT IdMenu,Nom,Libelle,Icone,Commentaire,Titre,Url,IdGroupMenu,Ordre,
	                                                               Visible,IdModule,IdAction
                                                            FROM _Menu
                                                            WHERE Visible = 1 AND IdGroupMenu = @IdGroupMenu
                                                            ORDER BY IdGroupMenu,Ordre";
        public static string SelectAllByGroupMenu { get; } = @"SELECT IdMenu,Nom,Libelle,Icone,Commentaire,Titre,Url,IdGroupMenu,Ordre,
	                                                               Visible,IdModule,IdAction
                                                            FROM _Menu
                                                            WHERE IdGroupMenu = @IdGroupMenu
                                                            ORDER BY IdGroupMenu,Ordre";

        public static string Insert { get; } = @"INSERT INTO _Menu (Nom,Libelle,Commentaire,Titre,Url,Ordre,IdGroupMenu,Visible,IdModule,IdAction,DefaultMenu)
                                                 VALUES (@Nom,@Libelle,@Commentaire,@Titre,@Url,@Ordre,@IdGroupMenu,@Visible,@IdModule,@IdAction,@DefaultMenu)";

        public static string Update { get; } = @"UPDATE _Menu 
                                                 SET Nom = @Nom, Libelle = @Libelle, Commentaire = @Commentaire, Titre = @Titre,
                                                     Url = @Url, Ordre = @Ordre,IdGroupMenu = @IdGroupMenu, Visible = @Visible, IdModule = @IdModule,
                                                     IdAction = @IdAction,DefaultMenu = @DefaultMenu
                                                 WHERE IdMenu = @IdMenu";

    }
}