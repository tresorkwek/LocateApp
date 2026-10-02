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
    public class ClaimModule : NancyModule
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(ClaimModule));

        public ClaimModule() : base("/claim")
        {
            this.RequiresAuthentication();

            Get("/", _ => this.RunHandler<int?>(GetClaim, null));
            Get("/select/{idProfil}", _ => this.RunHandler<int?>(GetClaim, (int?)_.idProfil));
            Get("/add/", _ => this.RunHandler(GetAddClaimForm));
            Get("/modify/{idProfil}", _ => this.RunHandler<int>(GetModifyClaimForm, (int)_.idProfil));

            Post("/add/", _ => this.RunHandler<AddUpdateClaimRequest>(UpSetClaim));
            Post("/modify/", _ => this.RunHandler<AddUpdateClaimRequest>(UpSetClaim));
        }

        private object GetClaim(int? idProfil = null)
        {
            List<Profil> listOfProfil = ProfilController.GetProfilWithClaims(idProfil);

            ClaimViewModel viewModelOfClaim = new ClaimViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Profils = listOfProfil
            };

            object view = View["ClaimView", viewModelOfClaim];
            int success = listOfProfil.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfProfil, viewModelOfClaim.Title, success);
        }

        private object GetAddClaimForm()
        {
            ClaimAddFrmViewModel viewModel = new ClaimAddFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString()
            };

            object view = View["ClaimAddFrmView", viewModel];
            int success = 1;

            return this.ResponseObject(view, null, viewModel.Title, success);
        }

        private object UpSetClaim(AddUpdateClaimRequest addClaimRequest)
        {
            
            bool result = ClaimControllers.UpSetClaim(addClaimRequest,IdentityController.GetUser(this.CurrentUserName()));

            string message;

            if (addClaimRequest.Update)
            {
                message = result ? $"Privillèges modifiés avec succès !" : $"La modification des privillèges a échoué !";
            }
            else
            {
                message = result ? $"Privillèges ajoutés avec succès !" : $"L'ajout des privillèges a échoué !";
            }
            
            string redirectUrl = $"/claim/";
            string messageTitle = addClaimRequest.Update ? "Modification Privillège" : "Ajout Privillège";
            int sucess = result ? Log.SUCCESS_CODE : Log.ERROR_CODE;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object GetModifyClaimForm(int idProfil)
        {            
            List<UserClaim> listOfUserClaim = ClaimControllers.GetClaimsByProfilForConfig(idProfil);

            ClaimModifyFrmViewModel viewModelOfProfil = new ClaimModifyFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = "GetClaim",
                UserClaims = listOfUserClaim
            };

            object view = View["ClaimModifyFrmView", viewModelOfProfil];
            int success = listOfUserClaim != null ? 1 : 0;
            string message = listOfUserClaim != null ? "Privilège non trouvé !" : viewModelOfProfil.Title;

            return this.ResponseObject(view, listOfUserClaim, message, success);
        }

    }
}