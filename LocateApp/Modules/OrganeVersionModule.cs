using System.Collections.Generic;
using System.Linq;
using LocateApp.Controllers;
using LocateApp.DataTransferObjects;
using LocateApp.Models;
using LocateApp.Utilities;
using LocateApp.ViewModels;
using Nancy;
using Nancy.Security;

namespace LocateApp.Modules
{
    /// <summary>Versions de l'organigramme : liste, création (avec copie), bascule de la version courante, suppression d'une version vide.</summary>
    public class OrganeVersionModule : NancyModule
    {
        public OrganeVersionModule() : base("/organe/version")
        {
            this.RequiresAuthentication();

            Get("/", _ => this.RunHandler<int?>(GetOrganeVersion, null));
            Get("/select/{id}", _ => this.RunHandler<int?>(GetOrganeVersion, (int?)_.id));
            Get("/add/", _ => this.RunHandler(GetAddOrganeVersionForm));
            Get("/modify/{id}", _ => this.RunHandler<int>(GetModifyOrganeVersionForm, (int)_.id));
            Get("/delete/{id}", _ => this.RunHandler<int>(DeleteOrganeVersion, (int)_.id));

            Post("/add/", _ => this.RunHandler<InsertOrganeVersionRequest>(AddOrganeVersion));
            Post("/modify/", _ => this.RunHandler<ModifyOrganeVersionRequest>(ModifyOrganeVersion));
        }

        private object GetOrganeVersion(int? id = null)
        {
            List<OrganeVersion> versions = OrganeVersionController.Select(id);

            OrganeVersionListViewModel viewModel = new OrganeVersionListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                OrganeVersions = versions
            };

            object view = View["OrganeVersionListView", viewModel];
            return this.ResponseObject(view, versions, viewModel.Title, versions.Count > 0 ? 1 : 0);
        }

        private object GetAddOrganeVersionForm()
        {
            OrganeVersionListViewModel viewModel = new OrganeVersionListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                OrganeVersions = OrganeVersionController.Select()
            };

            object view = View["OrganeVersionAddFrmView", viewModel];
            return this.ResponseObject(view, null, viewModel.Title, 1);
        }

        private object AddOrganeVersion(InsertOrganeVersionRequest values)
        {
            Identity user = IdentityController.GetUser(this.CurrentUserName());
            int id = OrganeVersionController.Insert(values, user, out int copies);

            if (id == 0)
            {
                return this.RedirectUrl("/organe/version/add/", "La création de la version a échoué.", 0, "Versions de l'organigramme");
            }

            string message = copies > 0
                ? $"Version {id} créée avec {copies} organe(s) recopié(s) depuis la version {values.CopierDepuis}."
                : $"Version {id} créée (vide). Ajoutez ses organes depuis l'organigramme.";

            if (values.Defaut && copies == 0)
            {
                message += " Elle n'a pas été définie comme courante car elle ne contient aucun organe.";
            }

            return this.RedirectUrl("/organe/version/", message, 1, "Versions de l'organigramme");
        }

        private object GetModifyOrganeVersionForm(int id)
        {
            OrganeVersion version = OrganeVersionController.Select(id).FirstOrDefault();
            if (version == null) return HttpStatusCode.NotFound;

            OrganeVersionModifyFrmViewModel viewModel = new OrganeVersionModifyFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                OrganeVersion = version
            };

            object view = View["OrganeVersionModifyFrmView", viewModel];
            return this.ResponseObject(view, version, viewModel.Title, 1);
        }

        private object ModifyOrganeVersion(ModifyOrganeVersionRequest values)
        {
            OrganeVersion avant = OrganeVersionController.Select(values.Id).FirstOrDefault();
            if (avant == null) return HttpStatusCode.NotFound;

            bool result = OrganeVersionController.Update(values, IdentityController.GetUser(this.CurrentUserName()));
            bool bascule = result && values.Defaut && !avant.Defaut && avant.NbOrgane > 0;

            string message = !result ? "La modification de la version a échoué."
                           : bascule ? $"La version {values.Id} est maintenant la version courante de l'organigramme."
                           : values.Defaut && !avant.Defaut ? $"Version {values.Id} enregistrée, mais elle ne peut pas devenir courante : elle ne contient aucun organe."
                           : $"Version {values.Id} enregistrée.";

            return this.RedirectUrl("/organe/version/", message, result ? 1 : 0, "Versions de l'organigramme");
        }

        private object DeleteOrganeVersion(int id)
        {
            OrganeVersion version = OrganeVersionController.Select(id).FirstOrDefault();
            if (version == null) return HttpStatusCode.NotFound;

            if (version.Defaut || version.NbOrgane > 0)
            {
                return this.RedirectUrl("/organe/version/", $"La version {id} ne peut pas être supprimée : elle est courante ou contient des organes.", 2, "Versions de l'organigramme");
            }

            bool result = OrganeVersionController.Delete(id, IdentityController.GetUser(this.CurrentUserName()));
            return this.RedirectUrl("/organe/version/", result ? $"Version {id} supprimée." : "La suppression a échoué.", result ? 1 : 0, "Versions de l'organigramme");
        }
    }
}
