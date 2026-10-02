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
    public class LocalModule : NancyModule
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(LocalModule));
        public LocalModule() : base("/local")
        {
            this.RequiresAuthentication();

            Get("/", _ => this.RunHandler<string>(GetLocal, null));
            Get("/select/{designation}", _ => this.RunHandler<string>(GetLocal, (string)_.designation));
            Get("/id/{id}", _ => this.RunHandler<int>(GetLocalById, (int)_.id));
            Get("/organe/list/", _ => this.RunHandler<string>(GetLocalViaOrganigramme, (string)_.codeOrgane));
            Get("/organe/list/{idInstitution_}/", _ => this.RunHandler<string>(GetLocalViaOrganigramme, (string)_.idInstitution_));
            Get("/organe/{codeOrgane}", _ => this.RunHandler<string>(GetLocalByCodeOrgane, (string)_.codeOrgane));
            Get("/qrcode/{qrCode}", _ => this.RunHandler<Guid>(GetLocalByQRCode, (Guid)_.qrCode));
            Get("/etiquette/{idEtiquette}", _ => this.RunHandler<long>(GetLocalByIdEtiquette, (long)_.idEtiquette));
            Get("/add/{codeOrgane}", _ => this.RunHandler<string>(GetAddLocalForm, (string)_.codeOrgane));
            Get("/modify/{id}", _ => this.RunHandler<long>(GetModifyLocalForm, (long)_.id));
            Get("/delete/{id}", _ => this.RunHandler<long>(DeleteLocal, (long)_.id));
            Get("/resetqrcode/{idLocal}", _ => this.RunHandler<long, bool>(DesaffectQRCode, (long)_.idLocal, true));
            Get("/liveqrcode/{idLocal}", _ => this.RunHandler<long, bool>(DesaffectQRCode, (long)_.idLocal, false));

            Post("/qrcode/", _ => this.RunHandler<AffectQRCodeToLocalRequest>(AffectQRCodeToLocal));           
            Post("/add/", _ => this.RunHandler<AddLocalRequest>(AddLocal));
            Post("/modify/", _ => this.RunHandler<ModifyLocalRequest>(ModifyLocal));

            Post("/sync/add/", _ => this.RunHandler<AddLocalSyncRequest>(AddLocalSync));
            Post("/sync/qrcode/", _ => this.RunHandler<AffectQRCodeToLocalRequest>(AffectQRCodeToLocal));

        }


        private object GetLocal(string designation = null)
        {
            List<Local> listOfLocaux = LocalController.SelectAll(designation,true);

            LocalListViewModel viewModelOfLocal = new LocalListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Locaux = listOfLocaux,
                Link = "/article/local/"
            };

            object view = View["LocalAllListView", viewModelOfLocal];
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
                Link = "/article/local/"
            };

            object view = View["LocalListView", viewModelOfLocalList];
            int success = listOfLocaux.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfLocaux, viewModelOfLocalList.Title, success);
        }

        private object GetLocalByQRCode(Guid qrCode)
        {
            // List<Local> locaux = LocalController.SelectByQrCode(qrCode);

            List<GetLocalRequest> locaux = LocalController.SelectRequestByQrCode(qrCode);
            GetLocalRequest local = locaux.FirstOrDefault();

            LocalViewModel viewModelOfLocal = new LocalViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Local = local
            };

            object view = View["LocalView", viewModelOfLocal];
            int success = locaux.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, locaux, viewModelOfLocal.Title, success);
        }

         private object GetLocalByIdEtiquette(long idEtiquette)
        {
            List<Local> locaux = LocalController.SelectByIdEtiquette(idEtiquette);

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
            string redirectUrl = $"/local/id/{affectQRCodeRequest.Id}";
            string messageTitle = "Affectation du QRCode à un local";
            int sucess = 0;
            string message;

            Etiquette etiquette = EtiquetteController.SelectPrintedByQrCode(affectQRCodeRequest.QrCode).FirstOrDefault();

            if (etiquette == null)
            {
                message = $"Vous tentez d'affecter un QRCode non imprimé par l'Hotel des Monnaies, Locate l'a rejeté !";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }
            else if (etiquette.IsUsed)
            {
                message = $"Vous tentez d'affecter un QRCode déjà utilisé, Locate l'a rejeté !";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            List<Local> locaux = LocalController.SelectById(affectQRCodeRequest.Id);

            bool result = LocalController.AffectQRCode(affectQRCodeRequest, IdentityController.GetUser(this.CurrentUserName()));

            redirectUrl = $"/local/id/{affectQRCodeRequest.Id}";
            messageTitle = "Affectation du QRCode à un local";
            sucess = result ? Log.SUCCESS_CODE : Log.ERROR_CODE;
            message = result ? $"{locaux[0].Designation} affecté au QRCode avec succès !" : $"L'affection du QRCode au local {locaux[0].Designation} a échouée !";

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object DesaffectQRCode(long idLocal, bool liberate)
        {
            string redirectUrl = $"/local/id/{idLocal}";
            string messageTitle = "Désaffectation du QRCode à un local";
            int sucess = 0;
            string message;

            Local local = LocalController.SelectById(idLocal).FirstOrDefault();

            if (local == null)
            {
                redirectUrl = $"/local/";
                message = $"Le local selectionné n'existe pas";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }
            else if (local.QrCode == null)
            {
                message = $"Le local {local.Designation} n'a pas de QRcode";
                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            bool result = LocalController.DesAffectQRCode(local, liberate, IdentityController.GetUser(this.CurrentUserName()));

            sucess = result ? 1 : Log.ERROR_CODE;
            message = result ? $"{local.Designation} désaffecté du QRCode avec succès !" : $"La désaffection du QRCode au local {local.Designation} a échouée !";

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
            int sucess = result ? 1 : 0;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object AddLocalSync(AddLocalSyncRequest addLocalValues)
        {

            long sucess = 0;

            try
            {
                sucess = LocalController.InsertSync(addLocalValues, IdentityController.GetUser(this.CurrentUserName()));
            }
            catch (Exception)
            {

               
            }
                       

            Organe organe = OrganeController.SelectById(addLocalValues.CodeOrgane);

            string message = sucess > 0 ? $"Local {addLocalValues.Designation} de l'organe {organe.Nom} inséré avec succès !" : $"L'insertion du local {addLocalValues.Designation} de l'organe {organe.Nom} a échouée !";
            string redirectUrl = $"/local/organe/{organe.Id}";
            string messageTitle = "Ajout d'un local";
            //int sucess = result ? 1 : 0;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object GetModifyLocalForm(long id)
        {
            Local local = LocalController.SelectById(id).FirstOrDefault();

            if (local == null)
            {
                string redirectUrl = "/local/";
                string messageTitle = "Modification d'un nouveau local";
                int sucess = Log.ERROR_CODE;
                string message = "Le local que vous avez choisi n'existe pas !";

                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            LocalAddOrModifyFrmViewModel viewModelOfLocal = new LocalAddOrModifyFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Local = local,
                IsSpace = false
            };

            object view = View["LocalModifyFrmView", viewModelOfLocal];
            int success = 1;

            return this.ResponseObject(view, null, viewModelOfLocal.Title, success);
        }


        private object ModifyLocal(ModifyLocalRequest modifyLocalValues)
        {

            bool result = LocalController.Update(modifyLocalValues, IdentityController.GetUser(this.CurrentUserName()));

            Organe organe = OrganeController.SelectById(modifyLocalValues.CodeOrgane);

            string message = result ? $"Local {modifyLocalValues.Designation} de l'organe {organe.Nom} modifié avec succès !" : $"La modification du local {modifyLocalValues.Designation} de l'organe {organe.Nom} a échouée !";
            string redirectUrl = $"/local/organe/{organe.Id}";
            string messageTitle = "Modification d'un local";
            int sucess = result ? Log.OK_CODE : Log.ERROR_CODE;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }


        private object DeleteLocal(long id)
        {
            string message = "Le local que vous avez renseigné n'existe pas !";
            bool result = false;
            string redirectUrl = $"/local/";

            Local local = LocalController.SelectById(id).FirstOrDefault();

            if (local != null)
            {
                result = LocalController.Delete(local.Id, IdentityController.GetUser(this.CurrentUserName()));

                Organe organe = OrganeController.SelectById(local.CodeOrgane);
                redirectUrl = $"/local/organe/{organe.Id}";
                message = result ? $"Local {local.Designation} de l'organe {organe.Nom} supprimé avec succès !" : $"La suppression du local {local.Designation} de l'organe {organe.Nom} a échouée !";
            }
            
            string messageTitle = "Supression d'un local";
            int sucess = result ? 1 : 0;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

    }
}