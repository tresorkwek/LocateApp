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
    public class MenuModule : NancyModule
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(MenuModule));

        public MenuModule() : base("/menu")
        {
            this.RequiresAuthentication();

            Get("/", _ => this.RunHandler<int?>(GetMenu,null));
            Get("/select/{idMenu}", _ => this.RunHandler<int?>(GetMenu, (int?)_.idMenu));
            Get("/groupmenu/{idGroupMenu}", _ => this.RunHandler<int>(GetMenuByGroupMenu, (int)_.idGroupMenu));
            Get("/add/", _ => this.RunHandler(GetAddMenuForm));
            Get("/modify/{idMenu}", _ => this.RunHandler<int>(GetModifyMenuForm, (int)_.idMenu));

            Post("/add/", _ => this.RunHandler<AddMenuRequest>(AddMenu));
            Post("/modify/", _ => this.RunHandler<ModifyMenuRequest>(ModifyMenu));
        }

        private object GetMenu(int? idMenu = null)
        {
            List<Menu> listOfMenu = MenuController.GetAllMenu(idMenu);

            MenuViewModel viewModelOfMenu = new MenuViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Menus = listOfMenu
            };

            object view = View["MenuView", viewModelOfMenu];
            int success = listOfMenu.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfMenu, viewModelOfMenu.Title, success);
        }

        private object GetMenuByGroupMenu(int idGroupMenu)
        {
            List<Menu> listOfMenu = MenuController.GetAllMenuByGroupMenu(idGroupMenu);

            MenuViewModel viewModelOfMenu = new MenuViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Menus = listOfMenu
            };

            object view = View["MenuView", viewModelOfMenu];
            int success = listOfMenu.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfMenu, viewModelOfMenu.Title, success);
        }

        private object GetAddMenuForm()
        {

            MenuAddFrmViewModel viewModelOfMenu = new MenuAddFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = "GetMenu"
            };

            object view = View["MenuAddFrmView", viewModelOfMenu];
            int success = 1;

            return this.ResponseObject(view, null, viewModelOfMenu.Title, success);
        }

        private object AddMenu(AddMenuRequest addMenuRequest)
        {

            bool result = MenuController.AddMenu(addMenuRequest, IdentityController.GetUser(this.CurrentUserName()));
            
            string redirectUrl = $"/menu/groupmenu/{addMenuRequest.IdGroupMenu}";
            string messageTitle = "Ajout Menu";
            int sucess = result ? Log.SUCCESS_CODE : Log.ERROR_CODE;
            string message = result? $"Menu {addMenuRequest.Nom} inséré avec succès !" : $"L'insertion du Menu {addMenuRequest.Nom} a échoué !";

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object GetModifyMenuForm(int idMenu)
        {
            Menu menu = MenuController.GetMenu(idMenu).FirstOrDefault();

            MenuModifyFrmViewModel viewModelOfMenu = new MenuModifyFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = "GetMenu",
                Menu = menu
            };

            object view = View["MenuModifyFrmView", viewModelOfMenu];
            int success = 1;

            return this.ResponseObject(view, menu, viewModelOfMenu.Title, success);
        }

        private object ModifyMenu(ModifyMenuRequest modifyMenuRequest)
        {
            bool result = MenuController.ModifyMenu(modifyMenuRequest, IdentityController.GetUser(this.CurrentUserName()));

            string redirectUrl = $"/menu/groupmenu/{modifyMenuRequest.IdGroupMenu}";
            string messageTitle = "Modification Menu";
            int sucess = result ? Log.SUCCESS_CODE : Log.ERROR_CODE;
            string message = result? $"Menu {modifyMenuRequest.Nom} modifié avec succès !" : $"La modification du Menu {modifyMenuRequest.Nom} a échoué !";

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

    }
}