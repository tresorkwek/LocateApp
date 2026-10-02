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
    public class CategorieModule : NancyModule
    {
        public CategorieModule() : base("/categorie")
        {
            this.RequiresAuthentication();

            Get("/", _ => this.RunHandler<string>(GetCategorie, null));
            Get("/select/{nom}", _ => this.RunHandler<string>(GetCategorie, (string)_.nom));
            Get("/id/{id}", _ => this.RunHandler<long>(GetCategorieById, (long)_.id));

            Get("/add/", _ => this.RunHandler(GetAddCategorieForm));
            Get("/modify/{id}", _ => this.RunHandler<long>(GetModifyCategorieForm, (long)_.id));
            Get("/delete/{id}", _ => this.RunHandler<long>(DeleteCategorie, (long)_.id));

            Post("/add/", _ => this.RunHandler<AddCategorieRequest>(AddCategorie));
            Post("/modify/", _ => this.RunHandler<ModifyCategorieRequest>(ModifyCategorie));

        }


        private object GetCategorie(string nom = null)
        {
            List<Categorie> listOfCategorie = CategorieController.Select(nom,true);

            CategorieListViewModel viewModelOfCategorie = new CategorieListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Categories = listOfCategorie
            };

            object view = View["CategorieListView", viewModelOfCategorie];
            int success = listOfCategorie.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfCategorie, viewModelOfCategorie.Title, success);
        }

        private object GetCategorieById(long id)
        {
            List<Categorie> listOfCategorie = CategorieController.SelectById(id);

            CategorieListViewModel viewModelOfCategorie = new CategorieListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Categories = listOfCategorie
            };

            object view = View["CategorieListView", viewModelOfCategorie];
            int success = listOfCategorie.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfCategorie, viewModelOfCategorie.Title, success);
        }

        private object GetAddCategorieForm()
        {

            CategorieAddOrModifyFrmViewModel viewModelOCategorie = new CategorieAddOrModifyFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString()
            };

            object view = View["CategorieAddFrmView", viewModelOCategorie];
            int success = 1;

            return this.ResponseObject(view, null, viewModelOCategorie.Title, success);
        }


        private object AddCategorie(AddCategorieRequest addCategorieValues)
        {
            bool result = CategorieController.Insert(addCategorieValues, IdentityController.GetUser(this.CurrentUserName()));

            string message = result ? $"Catégorie {addCategorieValues.Designation}  insérée avec succès !" : $"L'insertion de la catégorie {addCategorieValues.Designation} a échouée !";
            string redirectUrl = $"/categorie/";
            string messageTitle = "Ajout d'une catégorie";
            int sucess = result ? 1 : 0;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object GetModifyCategorieForm(long id)
        {
            Categorie categorie = CategorieController.SelectById(id).FirstOrDefault();

            CategorieAddOrModifyFrmViewModel viewModelOCategorie = new CategorieAddOrModifyFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Categorie = categorie
            };

            object view = View["CategorieModifyFrmView", viewModelOCategorie];
            int success = 1;

            return this.ResponseObject(view, null, viewModelOCategorie.Title, success);
        }

        private object ModifyCategorie(ModifyCategorieRequest modifyCategorieValues)
        {
            bool result = CategorieController.Update(modifyCategorieValues, IdentityController.GetUser(this.CurrentUserName()));

            string message = result ? $"Catégorie {modifyCategorieValues.Designation}  a été modifiée avec succès !" : $"La modification de la catégorie {modifyCategorieValues.Designation} a échouée !";
            string redirectUrl = $"/categorie/";
            string messageTitle = "Modification d'une catégorie";
            int sucess = result ? 1 : 0;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object DeleteCategorie(long id)
        {
            string message = "La catégorie que vous avez renseignée n'existe pas !";
            bool result = false;
            string redirectUrl = $"/categorie/";

            Categorie categorie = CategorieController.SelectById(id).FirstOrDefault();

            if (categorie != null)
            {
                result = CategorieController.Delete(categorie.Id, IdentityController.GetUser(this.CurrentUserName()));

                message = result ? $"Catégorie {categorie.Designation} supprimée avec succès !" : $"La suppression de la categorie {categorie.Designation} a échouée !";
            }

            string messageTitle = "Supression d'une catégorie";
            int sucess = result ? 1 : 0;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

    }
}