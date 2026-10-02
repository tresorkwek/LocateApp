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
    public class FournisseurModule : NancyModule
    {
        public FournisseurModule() : base("/fournisseur")
        {
            this.RequiresAuthentication();

            Get("/", _ => this.RunHandler<string>(GetFournisseur, null));
            Get("/select/{nom}", _ => this.RunHandler<string>(GetFournisseur, (string)_.nom));
            Get("/id/{id}", _ => this.RunHandler<long>(GetFournisseurById, (long)_.id));

            Get("/add/", _ => this.RunHandler(GetAddFournisseurForm));
            Get("/modify/{id}", _ => this.RunHandler<long>(GetModifyFournisseurForm, (long)_.id));

            Post("/add/", _ => this.RunHandler<AddFournisseurRequest>(AddFournisseur));
            Post("/modify/", _ => this.RunHandler<ModifyFournisseurRequest>(ModifyFournisseur));

        }


        private object GetFournisseur(string nom = null)
        {
            List<Fournisseur> listOfFournisseur = FournisseurController.Select(nom);

            FournisseurListViewModel viewModelOfFournisseur = new FournisseurListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Fournisseurs = listOfFournisseur
            };

            object view = View["FournisseurListView", viewModelOfFournisseur];
            int success = listOfFournisseur.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfFournisseur, viewModelOfFournisseur.Title, success);
        }

        private object GetFournisseurById(long id)
        {
            List<Fournisseur> listOfFournisseur = FournisseurController.SelectById(id);

            FournisseurListViewModel viewModelOfFournisseur = new FournisseurListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Fournisseurs = listOfFournisseur
            };

            object view = View["FournisseurListView", viewModelOfFournisseur];
            int success = listOfFournisseur.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfFournisseur, viewModelOfFournisseur.Title, success);
        }

        private object GetAddFournisseurForm()
        {

            ViewModel viewModelOCategorie = new ViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString()
            };

            object view = View["FournisseurAddFrmView", viewModelOCategorie];
            int success = 1;

            return this.ResponseObject(view, null, viewModelOCategorie.Title, success);
        }

        private object AddFournisseur(AddFournisseurRequest addFournisseurValues)
        {
            bool result = FournisseurController.Insert(addFournisseurValues, IdentityController.GetUser(this.CurrentUserName()));

            string message = result ? $"Fournisseur {addFournisseurValues.Nom}  inséré avec succès !" : $"L'insertion du fournisseur {addFournisseurValues.Nom} a échouée !";
            string redirectUrl = $"/fournisseur/";
            string messageTitle = "Ajout d'un fournisseur";
            int sucess = result ? 1 : 0;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object GetModifyFournisseurForm(long id)
        {
            Fournisseur fournisseur = FournisseurController.SelectById(id).FirstOrDefault();

            FournisseurModifyFrmViewModel viewModelOFamille = new FournisseurModifyFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Fournisseur = fournisseur
            };

            object view = View["FournisseurModifyFrmView", viewModelOFamille];
            int success = 1;

            return this.ResponseObject(view, null, viewModelOFamille.Title, success);
        }

        private object ModifyFournisseur(ModifyFournisseurRequest modifyFournisseurValues)
        {
            bool result = FournisseurController.Update(modifyFournisseurValues, IdentityController.GetUser(this.CurrentUserName()));

            string message = result ? $"Le fournisseur {modifyFournisseurValues.Nom}  a été modifié avec succès !" : $"La modification du fournisseur {modifyFournisseurValues.Nom} a échoué !";
            string redirectUrl = $"/fournisseur/";
            string messageTitle = "Modification d'un founisseur";
            int sucess = result ? 1 : 0;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

    }

}
