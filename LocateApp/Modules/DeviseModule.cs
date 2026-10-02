using System.Text.RegularExpressions;
using LocateApp.Controllers;
using LocateApp.DataTransferObjects;
using LocateApp.Models;
using LocateApp.Utilities;
using LocateApp.ViewModels;
using Nancy;
using Nancy.Security;

namespace LocateApp.Modules
{
    /// <summary>Devises et taux de change utilisés par les achats.</summary>
    public class DeviseModule : NancyModule
    {
        public DeviseModule() : base("/devise")
        {
            this.RequiresAuthentication();

            Get("/", _ => this.RunHandler(GetDevises));
            Post("/add/", _ => this.RunHandler<AddDeviseRequest>(AddDevise));
            Post("/modify/", _ => this.RunHandler<ModifyDeviseRequest>(ModifyDevise));
        }

        private object GetDevises()
        {
            DeviseListViewModel viewModel = new DeviseListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Devises = DeviseController.SelectAll()
            };

            object view = View["DeviseListView", viewModel];
            return this.ResponseObject(view, viewModel.Devises, viewModel.Title, viewModel.Devises.Count > 0 ? 1 : 0);
        }

        private object AddDevise(AddDeviseRequest values)
        {
            string erreur = Verifier(values);
            if (erreur == null && DeviseController.SelectByCode(values.Code) != null) erreur = $"La devise {values.Code.ToUpper()} existe déjà.";
            if (erreur != null) return this.RedirectUrl("/devise/", erreur, 0, "Devises");

            bool result = DeviseController.Insert(values, IdentityController.GetUser(this.CurrentUserName()));
            string message = result ? $"Devise {values.Code} ajoutée." : "L'ajout de la devise a échoué.";

            return this.RedirectUrl("/devise/", message, result ? 1 : 0, "Devises");
        }

        private object ModifyDevise(ModifyDeviseRequest values)
        {
            string erreur = Verifier(values);
            if (erreur != null) return this.RedirectUrl("/devise/", erreur, 0, "Devises");

            bool result = DeviseController.Update(values, IdentityController.GetUser(this.CurrentUserName()));
            string message = result ? $"Devise {values.Code} modifiée." : "La modification de la devise a échoué.";

            return this.RedirectUrl("/devise/", message, result ? 1 : 0, "Devises");
        }

        private static string Verifier(AddDeviseRequest values)
        {
            if (string.IsNullOrWhiteSpace(values.Code) || !Regex.IsMatch(values.Code.Trim(), "^[A-Za-z]{3}$")) return "Le code doit comporter exactement trois lettres (ex. USD, CDF, EUR).";
            if (string.IsNullOrWhiteSpace(values.Libelle)) return "Le libellé est obligatoire.";
            if (values.Taux <= 0) return "Le taux doit être strictement positif.";
            return null;
        }
    }
}
