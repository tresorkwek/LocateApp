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
    public class FamilleModule : NancyModule
    {
        public FamilleModule() : base("/famille")
        {
            this.RequiresAuthentication();

            Get("/", _ => this.RunHandler<string>(GetFamille, null));
            Get("/select/{nom}", _ => this.RunHandler<string>(GetFamille, (string)_.nom));
            Get("/id/{id}", _ => this.RunHandler<long>(GetFamilleById, (long)_.id));
            Get("/identifie/", _ => this.RunHandler(GetFamilleIdentifie));
            Get("/details/", _ => this.RunHandler<string>(GetFamilleDetails, null));
            Get("/details/{etat}", _ => this.RunHandler<string>(GetFamilleDetails, (string)_.etat));
            Get("/stat/pie/", _ => this.RunHandler(GetFamilleStatPie));
            Get("/stat/pie/legend/", _ => this.RunHandler(GetFamilleStatPieLegend));
            Get("/stat/radar/", _ => this.RunHandler(GetFamilleStatRadar));

            Get("/add/", _ => this.RunHandler(GetAddFamilleForm));
            Get("/modify/{id}", _ => this.RunHandler<long>(GetModifyFamilleForm, (long)_.id));

            Post("/add/", _ => this.RunHandler<AddFamilleRequest>(AddFamille));
            Post("/modify/", _ => this.RunHandler<ModifyFamillieRequest>(ModifyFamille));

        }


        private object GetFamille(string nom = null)
        {
            List<Famille> listOfFamile = FamilleController.Select(nom);

            FamilleListViewModel viewModelOfFamille = new FamilleListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Familles = listOfFamile
            };

            object view = View["FamilleListView", viewModelOfFamille];
            int success = listOfFamile.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfFamile, viewModelOfFamille.Title, success);
        }

        private object GetFamilleStatPie()
        {
            List<FamillieStatPieRequest> listOfFamileStat = FamilleController.SelectStatPie();

            FamilleListViewModel viewModelOfFamille = new FamilleListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Familles = null
            };

            object view = View["FamilleListView", viewModelOfFamille];
            int success = listOfFamileStat.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfFamileStat, viewModelOfFamille.Title, success);
        }

        private object GetFamilleStatPieLegend()
        {
            List<FamillieStatPieLegendRequest> listOfFamileStat = FamilleController.SelectStatPieLegend();

            FamilleListViewModel viewModelOfFamille = new FamilleListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Familles = null
            };

            object view = View["FamilleListView", viewModelOfFamille];
            int success = listOfFamileStat.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfFamileStat, viewModelOfFamille.Title, success);
        }

        private object GetFamilleStatRadar()
        {
            List<FamillieStatRadardRequest> listOfFamileStat = FamilleController.SelectStatRadar();

            FamilleListViewModel viewModelOfFamille = new FamilleListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Familles = null
            };

            object view = View["FamilleListView", viewModelOfFamille];
            int success = listOfFamileStat.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfFamileStat, viewModelOfFamille.Title, success);
        }

        private object GetFamilleIdentifie()
        {
            List<Famille> listOfFamile = FamilleController.SelectIdentifie();

            FamilleListViewModel viewModelOfFamille = new FamilleListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Familles = listOfFamile
            };

            object view = View["FamilleListView", viewModelOfFamille];
            int success = listOfFamile.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfFamile, viewModelOfFamille.Title, success);
        }

        private object GetFamilleDetails(string Etat = null)
        {
            List<Famille> listOfFamile = FamilleController.SelectWithNumber(Etat);

            FamilleListViewModel viewModelOfFamille = new FamilleListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Familles = listOfFamile
            };

            object view = View["FamilleListDetailsView", viewModelOfFamille];
            int success = listOfFamile.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfFamile, viewModelOfFamille.Title, success);
        }

        private object GetFamilleById(long id)
        {
            List<Famille> listOfFamille = FamilleController.SelectById(id);

            FamilleListViewModel viewModelOfFamille = new FamilleListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Familles = listOfFamille
            };

            object view = View["FamilleListView", viewModelOfFamille];
            int success = listOfFamille.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfFamille, viewModelOfFamille.Title, success);
        }

        private object GetAddFamilleForm()
        {

            ViewModel viewModel = new ViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString()
            };

            object view = View["FamilleAddFrmView", viewModel];
            int success = 1;

            return this.ResponseObject(view, null, viewModel.Title, success);
        }


        private object AddFamille(AddFamilleRequest addFamilleValues)
        {
            bool result = FamilleController.Insert(addFamilleValues, IdentityController.GetUser(this.CurrentUserName()));

            string message = result ? $"Famille {addFamilleValues.Nom}  insérée avec succès !" : $"L'insertion de la famille {addFamilleValues.Nom} a échouée !";
            string redirectUrl = $"/famille/";
            string messageTitle = "Ajout d'une famille";
            int sucess = result ? 1 : 0;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object GetModifyFamilleForm(long id)
        {
            Famille famille = FamilleController.SelectById(id).FirstOrDefault();

            FamilleModifyFrmViewModel viewModelOFamille = new FamilleModifyFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Famille = famille
            };

            object view = View["FamilleModifyFrmView", viewModelOFamille];
            int success = 1;

            return this.ResponseObject(view, null, viewModelOFamille.Title, success);
        }

        private object ModifyFamille(ModifyFamillieRequest modifyFamilleValues)
        {
            bool result = FamilleController.Update(modifyFamilleValues, IdentityController.GetUser(this.CurrentUserName()));

            string message = result ? $"La famille {modifyFamilleValues.Nom}  a été modifiée avec succès !" : $"La modification de la famille {modifyFamilleValues.Nom} a échouée !";
            string redirectUrl = $"/famille/";
            string messageTitle = "Modification d'une famille de bien";
            int sucess = result ? 1 : 0;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

    }
}