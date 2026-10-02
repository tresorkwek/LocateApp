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
    public class ObservationsModule : NancyModule
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(ObservationsModule));

        public ObservationsModule() : base("/observation")
        {
            this.RequiresAuthentication();

            Get("/", _ => this.RunHandler<int?>(GetObservations, null));
            Get("/select/{idObservation}", _ => this.RunHandler<int?>(GetObservations, (int?)_.idObservation));
            Get("/etat/{etat}", _ => this.RunHandler<string>(GetObservationsByEtat, (string)_.etat));
            Get("/stat/", _ => this.RunHandler(GetObservationsStat));
            Get("/add/", _ => this.RunHandler(GetAddObservationForm));
            Get("/modify/{idObservation}", _ => this.RunHandler<int>(GetModifyObservationForm, (int)_.idObservation));
            Get("/delete/{idObservation}", _ => this.RunHandler<int>(DeleteObservation, (int)_.idObservation));

            Post("/add/", _ => this.RunHandler<AddObservationRequest>(AddObservation));
            Post("/modify/", _ => this.RunHandler<ModifyObservationRequest>(ModifyObservation));
        }

        private object GetObservations(int? idObservation = null)
        {
            List<Observations> listOfObservation = ObservationsController.Select(idObservation);

            ObservationsListViewModel viewModelOfObservation = new ObservationsListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Observations = listOfObservation
            };

            object view = View["ObservationsListView", viewModelOfObservation];
            int success = listOfObservation.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfObservation, viewModelOfObservation.Title, success);
        }

        private object GetObservationsByEtat(string etat)
        {
            List<Observations> listOfObservation = ObservationsController.SelectByEtat(etat);

            ObservationsListViewModel viewModelOfObservation = new ObservationsListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Observations = listOfObservation
            };

            object view = View["ObservationsListView", viewModelOfObservation];
            int success = listOfObservation.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfObservation, viewModelOfObservation.Title, success);
        }
        private object GetObservationsStat()
        {
            List<SelectStatObservationRequest> listOfObservation = ObservationsController.SelectStat(true);

            ObservationsStatViewModel viewModelOfObservation = new ObservationsStatViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                ObservationsStat = listOfObservation
            };

            object view = View["ObservationsListView", viewModelOfObservation];
            int success = listOfObservation.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfObservation, viewModelOfObservation.Title, success);
        }

        private object GetAddObservationForm()
        {
            ViewModel viewModel = new ViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString()
            };

            object view = View["ObservationAddFrmView", viewModel];
            int success = 1;

            return this.ResponseObject(view, null, viewModel.Title, success);
        }

        private object AddObservation(AddObservationRequest addObservationValues)
        {            
            bool result = ObservationsController.Insert(addObservationValues,IdentityController.GetUser(this.CurrentUserName()));
            
            string redirectUrl = $"/observation/";
            string messageTitle =  "Ajout Observation";
            string message = result ? $"Observation ajoutés avec succès !" : $"L'ajout de l'observation a échoué !";
            int sucess = result ? 1 : Log.ERROR_CODE;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object ModifyObservation(ModifyObservationRequest modifyObservationValues)
        {
            bool result = ObservationsController.Update(modifyObservationValues, IdentityController.GetUser(this.CurrentUserName()));

            string redirectUrl = $"/observation/";
            string messageTitle = "Modification de l'Observation";
            string message = result ? $"Observation modifiée avec succès !" : $"La modification de l'observation a échouée !";
            int sucess = result ? 1 : Log.ERROR_CODE;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object DeleteObservation(int idObservation)
        {
            bool result = ObservationsController.Delete(idObservation, IdentityController.GetUser(this.CurrentUserName()));

            string redirectUrl = $"/observation/";
            string messageTitle = "Supression de l'Observation";
            string message = result ? $"Observation supprimée avec succès !" : $"La suppression de l'observation a échouée !";
            int sucess = result ? 1 : Log.ERROR_CODE;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object GetModifyObservationForm(int id)
        {            
            Observations observation = ObservationsController.Select(id).FirstOrDefault();

            ObservationModifyFrmViewModel viewModelOfObservation = new ObservationModifyFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Observations = observation
            };

            object view = View["ObservationModifyFrmView", viewModelOfObservation];
            int success = observation == null ? 0 : 1;
            string message = observation == null ? "Observation non trouvé !" : viewModelOfObservation.Title;

            return this.ResponseObject(view, observation, message, success);
        }

    }
}