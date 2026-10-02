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
    public class OrganeTypeModule : NancyModule
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(OrganeTypeModule));

        public OrganeTypeModule() : base("/organe/type")
        {
            this.RequiresAuthentication();

            Get("/", _ => this.RunHandler<int?>(GetOrganeType, null));
            Get("/select/{id}", _ => this.RunHandler<int?>(GetOrganeType, (int?)_.id));
            Get("/add/", _ => this.RunHandler(GetAddOrganeTypeForm));
            Get("/modify/{id}", _ => this.RunHandler<int>(GetModifyOrganeTypeForm, (int)_.id));

            Post("/add/", _ => this.RunHandler<InsertOrganeTypeRequest>(AddOrganeType));
            Post("/modify/", _ => this.RunHandler<ModifyOrganeTypeRequest>(ModifyOrganeType));
        }

        private object GetOrganeType(int? id = null)
        {
            List<OrganeType> listOfOrganeType = OrganeTypeController.Select(id);

            OrganeTypeListViewModel viewModelOfOrganeType = new OrganeTypeListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                OrganeTypes = listOfOrganeType
            };

            object view = View["OrganeTypeListView", viewModelOfOrganeType];
            int success = listOfOrganeType.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfOrganeType, viewModelOfOrganeType.Title, success);
        }

        private object GetAddOrganeTypeForm()
        {
            ViewModel viewModel = new ViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString()
            };

            object view = View["OrganeTypeAddFrmView", viewModel];
            int success = 1;

            return this.ResponseObject(view, null, viewModel.Title, success);
        }

        private object AddOrganeType(InsertOrganeTypeRequest addOrganeTypeValues)
        {

            bool result = OrganeTypeController.Insert(addOrganeTypeValues,IdentityController.GetUser(this.CurrentUserName()));

            string message = result? $"Type d'organe {addOrganeTypeValues.Nom} inséré avec succès !" : $"L'insertion du Type d'ogane {addOrganeTypeValues.Nom} échouée !";
            string redirectUrl = $"/organe/type/";
            string messageTitle = "Ajout d'un type d'organe";
            int sucess = result ? 1 : 0;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object GetModifyOrganeTypeForm(int id)
        {
            OrganeType organeType = OrganeTypeController.Select(id).FirstOrDefault();

            OrganeTypeModifyFrmViewModel viewModelOfOrganeType = new OrganeTypeModifyFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                OrganeType = organeType
            };

            object view = View["OrganeTypeModifyFrmView", viewModelOfOrganeType];
            int success = organeType != null ? 1 : 0;
            string message = organeType != null ? "Type d'organe non trouvé !" : viewModelOfOrganeType.Title;

            return this.ResponseObject(view, organeType, message, success);
        }

        private object ModifyOrganeType(ModifyOrganeTypeRequest modifyOrganeTypeRequest)
        {
            bool result = OrganeTypeController.Update(modifyOrganeTypeRequest, IdentityController.GetUser(this.CurrentUserName()));

            string redirectUrl = $"/organe/type/";
            string messageTitle = "Modification du type d'organe";
            int sucess = result ? Log.SUCCESS_CODE : Log.ERROR_CODE;
            string message = result ? $"Type d'organe {modifyOrganeTypeRequest.Nom} modifié avec succès !" : $"La modification du type d'organe {modifyOrganeTypeRequest.Nom} a échouée !";

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

    }
}