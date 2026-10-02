using LocateApp.Controllers;
using LocateApp.DataTransferObjects;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy.ModelBinding;
using Nancy.Security;
using LocateApp.Utilities;
using LocateApp.ViewModels;
using LocateApp.Models;

namespace LocateApp.Modules
{
    public class GroupMenuModule : NancyModule
    {
        public GroupMenuModule() : base("/groupmenu")
        {
            this.RequiresAuthentication();

            Get("/", _ => this.RunHandler<int?>(GetGroupMenu,null));
            Get("/select/{idGroupMenu}", _ => this.RunHandler<int?>(GetGroupMenu, (int?)_.idGroupMenu));
            Get("/visibility/{idGroupMenu}", _ => this.RunHandler<int, bool>(Visibility, (int)_.idGroupMenu,true));
            Get("/invisibility/{idGroupMenu}", _ => this.RunHandler<int,bool>(Visibility, (int)_.idGroupMenu,false));

        }

        private object GetGroupMenu(int? idGroupMenu = null)
        {
            List<GroupMenu> listOfGroupMenu = GroupMenuController.GetAllGroupMenu(idGroupMenu);

            GroupMenuViewModel viewModelOGroupMenu = new GroupMenuViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                GroupMenus = listOfGroupMenu
            };

            object view = View["GroupMenuView", viewModelOGroupMenu];
            int success = listOfGroupMenu.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfGroupMenu, viewModelOGroupMenu.Title, success);
        }

        private object Visibility(int idGroupMenu, bool visible)
        {
            bool result = GroupMenuController.Visible(idGroupMenu, visible);
            GroupMenu groupMenu = GroupMenuController.GetAllGroupMenu(idGroupMenu).FirstOrDefault();

            string message = visible? $"Le Groupe Menu {groupMenu.Nom} est maintenant visible " : $"Le Groupe Menu {groupMenu.Nom} est maintenant invisible ";
            string redirectUrl = $"/groupmenu/";
            int success = result ? 1 : 0;
            string title = "Visibilité d'un Groupe Menu";

            return this.RedirectUrl(redirectUrl, message, success, title);

        }

    }
}