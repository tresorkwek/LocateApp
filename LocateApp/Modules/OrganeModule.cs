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
    public class OrganeModule : NancyModule
    {
        public OrganeModule() : base("/organe")
        {
            this.RequiresAuthentication();

            Get("/", _ => this.RunHandler<string>(GetOrgane, null));
            Get("/select/", _ => this.RunHandler<string>(GetOrgane, null));
            Get("/select/{code}", _ => this.RunHandler<string>(GetOrgane, (string)_.code));
            Get("/autonome/", _ => this.RunHandler(GetEntiteAutonome));
            Get("/entite/", _ => this.RunHandler<string>(GetOrganeEntite, null));
            Get("/entite/{code}", _ => this.RunHandler<string>(GetOrganeEntite, (string)_.code));
            Get("/organigramme/", _ => this.RunHandler<string>(GetOrganigramme, null));
            Get("/organigramme/{id}", _ => this.RunHandler<string>(GetOrganigramme, (string)_.id));
            Get("/organigramme/version/{id}", _ => this.RunHandler<string>(GetOrganigrammeByVersion, (string)_.id));

            Get("/config/organigramme/", _ => this.RunHandler<string,bool?>(GetOrganigrammeConfig, null,null));
            Get("/config/organigramme/{id}", _ => this.RunHandler<string,bool?>(GetOrganigrammeConfig, (string)_.id,null));

            Get("/add/organigramme/", _ => this.RunHandler<string, bool?>(GetOrganigrammeConfig, null, true));
            Get("/add/organigramme/{id}", _ => this.RunHandler<string, bool?>(GetOrganigrammeConfig, (string)_.id, true));

            Get("/modify/organigramme/", _ => this.RunHandler<string, bool?>(GetOrganigrammeConfig, null, false));
            Get("/modify/organigramme/{id}", _ => this.RunHandler<string, bool?>(GetOrganigrammeConfig, (string)_.id, false));

            Get("/add/", _ => this.RunHandler<string>(GetAddOrganeForm, null));
            Get("/add/{id}", _ => this.RunHandler<string>(GetAddOrganeForm, (string)_.id));
            Get("/modify/{id}", _ => this.RunHandler<string>(GetModifyOrganeForm, (string)_.id));

            Post("/add/", _ => this.RunHandler<UpSetOrganeRequest>(AddOrgane));
            Post("/modify/", _ => this.RunHandler<UpSetOrganeRequest>(ModifyOrgane));

            Post("/organigramme/reorder", _ => this.RunHandler<OrganigrammeReorderRequest>(ReorderOrganigramme));
            Get("/delete/{id}", _ => this.RunHandler<string>(DeleteOrgane, (string)_.id));

        }

        private object GetOrgane(string valueToSelect = null)
        {
            List<Organe> listOfOrgane = OrganeController.Select(valueToSelect);

            OrganeViewModel viewModelOfLocal = new OrganeViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Organes = listOfOrgane
            };

            object view = View["OrganigrammeView", viewModelOfLocal];
            int success = listOfOrgane.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfOrgane, viewModelOfLocal.Title, success);
        }

        private object GetEntiteAutonome()
        {
            List<Organe> listOfOrgane = OrganeController.SelectEntite();

            OrganeViewModel viewModelOfLocal = new OrganeViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Organes = listOfOrgane
            };

            object view = View["OrganigrammeView", viewModelOfLocal];
            int success = listOfOrgane.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfOrgane, viewModelOfLocal.Title, success);
        }

        private object GetOrganeEntite(string idStructure = null)
        {
            List<Organe> listOfOrgane = OrganeController.SelectOrganesStructure(idStructure);

            OrganeViewModel viewModelOfLocal = new OrganeViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Organes = listOfOrgane
            };

            object view = View["OrganigrammeView", viewModelOfLocal];
            int success = listOfOrgane.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfOrgane, viewModelOfLocal.Title, success);
        }

        private object GetOrganigramme(string id = null)
        {
            List<Organe> organigramme = OrganeController.Organigramme(id);

            OrganigrammeViewModel viewModelOfLocal = new OrganigrammeViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Organigramme = organigramme
            };

            object view = View["OrganigrammeView", viewModelOfLocal];
            int success = organigramme.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, organigramme, viewModelOfLocal.Title, success);
        }

        private object GetOrganigrammeByVersion(string version)
        {
            List<Organe> organigramme = OrganeController.OrganigrammeByVersion(version);

            OrganigrammeViewModel viewModelOfLocal = new OrganigrammeViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Organigramme = organigramme
            };

            object view = View["OrganigrammeConfigView", viewModelOfLocal];
            int success = organigramme.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, organigramme, viewModelOfLocal.Title, success);
        }

        private object GetOrganigrammeConfig(string id = null, bool? add = null)
        {
            List<Organe> organigramme = OrganeController.OrganigrammeForConfig(id);

            string link = "/local/organe/";
            bool showOrganeParentButon = false;

            if (add != null)
            {
                link = (bool)add ? "/organe/add/" : "/organe/modify/";
                showOrganeParentButon = id == null && add == true;
            }

            OrganigrammeViewModel viewModelOfLocal = new OrganigrammeViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Organigramme = organigramme,
                Link = link,
                OrganeParent = showOrganeParentButon
            };

            object view = View["OrganigrammeConfigView", viewModelOfLocal];
            int success = organigramme.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, organigramme, viewModelOfLocal.Title, success);
        }

        private object GetAddOrganeForm(string id = null)
        {

            Organe organe = id == null ? null : OrganeController.SelectById(id);

            if (id != null && organe == null)
            {
                string redirectUrl = $"/organe/config/organigramme/";
                string messageTitle = "Création d'un organe";
                int sucess = 0;

                string message = $"Vous avez selectionné un organe parent inexistant";

                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            OrganeAddFrmViewModel viewModel = new OrganeAddFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                OrganeParent = organe,
                OrdreInterne = organe == null ? 1 : OrganeController.SelectMaxOrdreInterne(organe.IdStructure)
        };

            object view = View["OrganeAddFrmView", viewModel];
            int success = 1;

            return this.ResponseObject(view, null, viewModel.Title, success);
        }

        private object AddOrgane(UpSetOrganeRequest upSetOrganeValues)
        {

            OrganeVersion organeVersion = OrganeVersionController.SelectDefault();
            upSetOrganeValues.Version = organeVersion.Id;

            // Un sous-organe appartient à la version de son parent (permet de compléter une version archivée ou en préparation).
            Organe parent = string.IsNullOrWhiteSpace(upSetOrganeValues.IdOrganeParent) ? null : OrganeController.SelectById(upSetOrganeValues.IdOrganeParent);
            if (parent != null) upSetOrganeValues.Version = parent.Version;

            bool result = OrganeController.Insert(upSetOrganeValues, IdentityController.GetUser(this.CurrentUserName()));

            string message = result ? $"Organe {upSetOrganeValues.Nom} inséré avec succès !" : $"L'insertion de l'organe {upSetOrganeValues.Nom} a échouée !";
            string redirectUrl = upSetOrganeValues.Version == organeVersion.Id ? "/organe/config/organigramme/" : $"/organe/organigramme/version/{upSetOrganeValues.Version}";
            string messageTitle = "Ajout d'un organe";
            int sucess = result ? 1 : 0;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object GetModifyOrganeForm(string id)
        {
            Organe organe = OrganeController.SelectById(id);

            if (organe == null)
            {
                string redirectUrl = $"/organe/config/organigramme/";
                string messageTitle = "Modification d'un organe";
                int sucess = 0;

                string message = $"Vous avez selectionné un organe inexistant";

                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            OrganeModifyFrmViewModel viewModel = new OrganeModifyFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Organe = organe
            };

            object view = View["OrganeModifyFrmView", viewModel];
            int success = 1;

            return this.ResponseObject(view, null, viewModel.Title, success);
        }

        private object ModifyOrgane(UpSetOrganeRequest upSetOrganeValues)
        {
            if (upSetOrganeValues.Version == 0)
            {
                OrganeVersion organeVersion = OrganeVersionController.SelectDefault();
                upSetOrganeValues.Version = organeVersion.Id;
            }

            bool result = OrganeController.Update(upSetOrganeValues, IdentityController.GetUser(this.CurrentUserName()));

            string message = result ? $"Organe {upSetOrganeValues.Nom} modifié avec succès !" : $"La modification de l'organe {upSetOrganeValues.Nom} a échoué !";
            int versionCourante = OrganeVersionController.SelectDefault()?.Id ?? upSetOrganeValues.Version;
            string redirectUrl = upSetOrganeValues.Version == versionCourante ? "/organe/config/organigramme/" : $"/organe/organigramme/version/{upSetOrganeValues.Version}";
            string messageTitle = "Modification d'un organe";
            int sucess = result ? 1 : 0;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

    
        /// <summary>Persistance du glisser-déposer (appel AJAX de la vue OrganigrammeConfigView).</summary>
        private object ReorderOrganigramme(OrganigrammeReorderRequest request)
        {
            bool result = OrganeController.ReorderOrganigramme(request, IdentityController.GetUser(this.CurrentUserName()));
            return this.Response.AsJson(new { success = result });
        }

        /// <summary>Suppression d'un organe sans rattachement ; sinon on explique ce qui bloque.</summary>
        private object DeleteOrgane(string id)
        {
            Organe organe = OrganeController.SelectById(id);
            if (organe == null) return HttpStatusCode.NotFound;

            string retour = organe.Version == (OrganeVersionController.SelectDefault()?.Id ?? organe.Version) ? "/organe/config/organigramme/" : $"/organe/organigramme/version/{organe.Version}";
            OrganeDependances dep = OrganeController.Dependances(id);

            if (!dep.EstSupprimable)
            {
                return this.RedirectUrl(retour, $"L'organe {organe.Nom} ne peut pas être supprimé : {dep.Detail}. Désactivez-le plutôt.", 2, "Suppression d'un organe");
            }

            bool result = OrganeController.Delete(id, IdentityController.GetUser(this.CurrentUserName()));
            string message = result ? $"Organe {organe.Nom} supprimé." : $"La suppression de l'organe {organe.Nom} a échoué.";

            return this.RedirectUrl(retour, message, result ? 1 : 0, "Suppression d'un organe");
        }

}
}