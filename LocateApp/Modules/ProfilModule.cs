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
    public class ProfilModule : NancyModule
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(ProfilModule));

        public ProfilModule() : base("/profil")
        {
            this.RequiresAuthentication();

            Get("/", _ => this.RunHandler<int?>(GetProfil,null));
            Get("/select/{idProfil}", _ => this.RunHandler<int?>(GetProfil, (int?)_.idProfil));
           // Get("/groupmenu/{idGroupMenu}", _ => this.RunHandler<int>(GetMenuByGroupMenu, (int)_.idGroupMenu));
            Get("/add/", _ => this.RunHandler(GetAddProfilForm));
            Get("/modify/{idProfil}", _ => this.RunHandler<int>(GetModifyProfilForm, (int)_.idProfil));

            Post("/add/", _ => this.RunHandler<AddProfilRequest>(AddProfil));
            Post("/modify/", _ => this.RunHandler<ModifyProfilRequest>(ModifyProfil));
        }

        private object GetProfil(int? idProfil = null)
        {
            List<Profil> listOfProfil = ProfilController.GetProfil(idProfil);

            ProfilViewModel viewModelOProfil = new ProfilViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Profils = listOfProfil
            };

            object view = View["ProfilView", viewModelOProfil];
            int success = listOfProfil.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfProfil, viewModelOProfil.Title, success);
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

        private object GetAddProfilForm()
        {
            ViewModel viewModel = new ViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = "GetPorfil"
            };

            object view = View["ProfilAddFrmView", viewModel];
            int success = 1;

            return this.ResponseObject(view, null, viewModel.Title, success);
        }

        private object AddProfil(AddProfilRequest addPorfilRequest)
        {

            bool result = ProfilController.AddProfil(addPorfilRequest,IdentityController.GetUser(this.CurrentUserName()));

            string message = result? $"Profl {addPorfilRequest.Nom} inséré avec succès !" : $"Profl {addPorfilRequest.Nom} échoué !";
            string redirectUrl = $"/profil/";
            string messageTitle = "Ajout Profil";
            int sucess = result ? Log.OK_CODE : Log.ERROR_CODE;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object GetModifyProfilForm(int idProfil)
        {
            Profil profil = ProfilController.GetProfil(idProfil).FirstOrDefault();

            ProfilModifyFrmViewModel viewModelOfProfil = new ProfilModifyFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = "GetProfil",
                Profil = profil
            };

            object view = View["ProfilModifyFrmView", viewModelOfProfil];
            int success = profil != null ? 1 : 0;
            string message = profil != null ? "Porfil non trouvé !" : viewModelOfProfil.Title;

            return this.ResponseObject(view, profil, message, success);
        }

        private object ModifyProfil(ModifyProfilRequest modifyProfilRequest)
        {
            bool result = ProfilController.ModifyProfil(modifyProfilRequest, IdentityController.GetUser(this.CurrentUserName()));

            string redirectUrl = $"/profil/";
            string messageTitle = "Modification Profil";
            int sucess = result ? Log.SUCCESS_CODE : Log.ERROR_CODE;
            string message = result? $"Profil {modifyProfilRequest.Nom} modifié avec succès !" : $"La modification du profil {modifyProfilRequest.Nom} a échoué !";

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

    }
}