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
    public class EspaceModule : NancyModule
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(EspaceModule));
        public EspaceModule() : base("/espace")
        {
            this.RequiresAuthentication();

            Get("/", _ => this.RunHandler<string>(GetEspace, null));
            Get("/select/{designation}", _ => this.RunHandler<string>(GetEspace, (string)_.designation));
            Get("/id/{id}", _ => this.RunHandler<int>(GetLocalById, (int)_.id));
            Get("/organe/", _ => this.RunHandler<string>(GetLocalViaOrganigramme, (string)_.codeOrgane));
            Get("/organe/{codeOrgane}", _ => this.RunHandler<string>(GetLocalByCodeOrgane, (string)_.codeOrgane));
            Get("/qrcode/{qrCode}", _ => this.RunHandler<Guid>(GetLocalByQRCode, (Guid)_.qrCode));
            Get("/add/{codeOrgane}", _ => this.RunHandler<string>(GetAddLocalForm, (string)_.codeOrgane));
            //Get("/modify/{id}", _ => this.RunHandler<int>(GetModifyProfilForm, (int)_.idProfil));//Todo

            Post("/qrcode/", _ => this.RunHandler<AffectQRCodeToLocalRequest>(AffectQRCodeToLocal));           

            Post("/add/", _ => this.RunHandler<AddLocalRequest>(AddLocal));
            //Post("/modify/", _ => this.RunHandler<ModifyProfilRequest>(ModifyProfil));//TODO

        }


        private object GetEspace(string designation = null)
        {
            List<Local> listOfLocaux = LocalController.SelectAll(designation);

            LocalListViewModel viewModelOfLocal = new LocalListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Locaux = listOfLocaux
            };

            object view = View["LocalListView", viewModelOfLocal];
            int success = listOfLocaux.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfLocaux, viewModelOfLocal.Title, success);
        }


        private object GetLocalById(int id)
        {
            List<Local> listOfLocaux = LocalController.SelectById(id);

            LocalListViewModel viewModelOfLocal = new LocalListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Locaux = listOfLocaux
            };

            object view = View["LocalListView", viewModelOfLocal];
            int success = listOfLocaux.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfLocaux, viewModelOfLocal.Title, success);
        }

        private object GetLocalViaOrganigramme(string codeOrgane = null)
        {
            List<Organe> organigramme = OrganeController.Organigramme(codeOrgane);

            OrganigrammeViewModel viewModelOfLocal = new OrganigrammeViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Organigramme = organigramme,
                Link = "/local/organe/"
            };

            object view = View["LocalOrganigrammeView", viewModelOfLocal];
            int success = organigramme.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, organigramme, viewModelOfLocal.Title, success);
        }

        private object GetLocalByCodeOrgane(string codeOrgane)
        {
            Organe organe = OrganeController.SelectById(codeOrgane);

            if (organe == null)
            {
                string redirectUrl = "/local/";
                string messageTitle = "Liste des locaux";
                int sucess = Log.ERROR_CODE;
                string message = "L'organe que vous avez renseigné n'existe pas !";

                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            List<Local> listOfLocaux = LocalController.SelectAllByCodeOrgane(codeOrgane);

            LocalListViewModel viewModelOfLocalList = new LocalListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Locaux = listOfLocaux,
                Organe = organe,
                Link = "/local/add/" + codeOrgane
            };

            object view = View["LocalListView", viewModelOfLocalList];
            int success = listOfLocaux.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfLocaux, viewModelOfLocalList.Title, success);
        }

        private object GetLocalByQRCode(Guid qrCode)
        {
            List<Local> locaux = LocalController.SelectByQrCode(qrCode);

            LocalListViewModel viewModelOfLocal = new LocalListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Locaux = locaux
            };

            object view = View["LocalListView", viewModelOfLocal];
            int success = locaux.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, locaux, viewModelOfLocal.Title, success);
        }

        private object AffectQRCodeToLocal(AffectQRCodeToLocalRequest affectQRCodeRequest)
        {

            List<Local> locaux = LocalController.SelectById(affectQRCodeRequest.Id);
            bool result = LocalController.AffectQRCode(affectQRCodeRequest, IdentityController.GetUser(this.CurrentUserName()));

            string redirectUrl = $"/local/id/{affectQRCodeRequest.Id}";
            string messageTitle = "Affectation du QRCode à un local";
            int sucess = result ? Log.SUCCESS_CODE : Log.ERROR_CODE;
            string message = result ? $"{locaux[0].Designation} affecté au QRCode avec succès !" : $"L'affection du QRCode au local {locaux[0].Designation} a échouée !";

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object GetAddLocalForm(string codeOrgane)
        {
            Organe organe = OrganeController.SelectById(codeOrgane);

            if(organe == null)
            {
                string redirectUrl = "/local/";
                string messageTitle = "Ajout d'un nouveau local";
                int sucess = Log.ERROR_CODE;
                string message = "Le local que vous avez choisi n'existe pas !";

                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            LocalAddOrModifyFrmViewModel viewModelOfLocal = new LocalAddOrModifyFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Organe = organe,
                IsSpace = false
            };

            object view = View["LocalAddFrmView", viewModelOfLocal];
            int success = 1;

            return this.ResponseObject(view, null, viewModelOfLocal.Title, success);
        }

        private object AddLocal(AddLocalRequest addLocalValues)
        {

            bool result = LocalController.Insert(addLocalValues, IdentityController.GetUser(this.CurrentUserName()));

            Organe organe = OrganeController.SelectById(addLocalValues.CodeOrgane);

            string message = result ? $"Local {addLocalValues.Designation} de l'organe {organe.Nom} inséré avec succès !" : $"L'insertion du local {addLocalValues.Designation} de l'organe {organe.Nom} a échouée !";
            string redirectUrl = $"/local/organe/{organe.Id}";
            string messageTitle = "Ajout d'un local";
            int sucess = result ? Log.OK_CODE : Log.ERROR_CODE;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }
    }
}