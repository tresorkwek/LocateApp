
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Repository
{
    public static class SqlMenuToShow
    {
		public static string SelectAll { get; } = @"SELECT _GroupMenu.IdGroupMenu,_GroupMenu.Nom as NomGroup,_GroupMenu.Libelle As LibelleGroup,_GroupMenu.Icone,
														   _GroupMenu.Ordre AS OrdreGroup,_GroupMenu.Visible AS VisibleGroup,_Section.IdSection,_Section.Nom As Section,	   
														   _Menu.IdMenu, _Menu.Nom AS NomMenu,_Menu.Libelle AS MenuLibelle,Commentaire,Titre,_Menu.Url,_Menu.Ordre AS OrdreMenu,
														   _Menu.Visible AS VisibleMenu,_Module.IdModule,_Module.Nom AS NomModule,_Action.IdAction,_Action.Nom AS NomAction
														  ,_Profil.idProfil ,_Profil.Nom AS NomProfil,_Claim.TableName,_Claim.Champ,_Claim.Valeur
	   
													FROM _GroupMenu INNER JOIN _Menu ON _GroupMenu.IdGroupMenu = _Menu.IdGroupMenu
														  INNER JOIN _Module ON _Menu.IdModule = _Module.IdModule
														  INNER JOIN _Action ON _Menu.IdAction = _Action.IdAction
														  LEFT JOIN _Claim ON _Menu.idMenu = _Claim.idMenu
														  LEFT JOIN _Profil ON _Claim.IdProfil = _Profil.IdProfil
														  LEFT JOIN _Section ON _GroupMenu.IdSection = _Section.IdSection
													WHERE _Menu.Visible = 1 
													ORDER BY _GroupMenu.Ordre,_Menu.Ordre";


		public static string SelectByProfil { get; } = @"SELECT _GroupMenu.IdGroupMenu,_GroupMenu.Nom as NomGroup,_GroupMenu.Libelle As LibelleGroup,_GroupMenu.Icone,
														   _GroupMenu.Ordre AS OrdreGroup,_GroupMenu.Visible AS VisibleGroup,_Section.IdSection,_Section.Nom As Section,	   
														   _Menu.IdMenu, _Menu.Nom AS NomMenu,_Menu.Libelle AS MenuLibelle,Commentaire,Titre,_Menu.Url,_Menu.Ordre AS OrdreMenu,
														   _Menu.Visible AS VisibleMenu,_Module.IdModule,_Module.Nom AS NomModule,_Action.IdAction,_Action.Nom AS NomAction
														  ,_Profil.idProfil ,_Profil.Nom AS NomProfil,_Claim.TableName,_Claim.Champ,_Claim.Valeur
	   
													FROM _GroupMenu INNER JOIN _Menu ON _GroupMenu.IdGroupMenu = _Menu.IdGroupMenu
														  INNER JOIN _Module ON _Menu.IdModule = _Module.IdModule
														  INNER JOIN _Action ON _Menu.IdAction = _Action.IdAction
														  LEFT JOIN _Claim ON _Menu.idMenu = _Claim.idMenu
														  LEFT JOIN _Profil ON _Claim.IdProfil = _Profil.IdProfil
														  LEFT JOIN _Section ON _GroupMenu.IdSection = _Section.IdSection
													WHERE _Menu.Visible = 1 AND _Profil.IdProfil = @IdProfil
													ORDER BY _GroupMenu.Ordre,_Menu.Ordre";

	}
}