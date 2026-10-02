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
    public class CommandeModule : NancyModule
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(CommandeModule));

        public CommandeModule() : base("/commande")
        {
            this.RequiresAuthentication();

            Get("/", _ => this.RunHandler<Guid?>(GetCommande,null));
            Get("/select/{id}", _ => this.RunHandler<Guid?>(GetCommande, (Guid?)_.id));
            Get("/add/", _ => this.RunHandler(GetCommandeAddForm));
            Get("/modify/{id}", _ => this.RunHandler<Guid>(GetModifyCommandeForm, (Guid)_.id));

            Post("/add/", _ => this.RunHandler<CommandeInsertRequest>(AddCommande));
            Post("/modify/", _ => this.RunHandler<CommandeUpdateRequest>(ModifyCommande));
        }

        private object GetCommande(Guid? id = null)
        {
            List<Commande> listOfCommande = CommandeController.Select(id);

            CommandeViewModel viewModelOCommande = new CommandeViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Commandes = listOfCommande
            };

            object view = View["CommandeListView", viewModelOCommande];
            int success = listOfCommande.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfCommande, viewModelOCommande.Title, success);
        }

        private object GetCommandeAddForm()
        {
            CommandeAddFormViewModel viewModel = new CommandeAddFormViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString()
            };

            object view = View["CommandeAddFormView", viewModel];
            int success = 1;

            return this.ResponseObject(view, null, viewModel.Title, success);
        }

        private object AddCommande(CommandeInsertRequest insertCommandeRequest)
        {
            //bool result = CommandeController.Insert(insertCommandeRequest, IdentityController.GetUser(this.CurrentUserName()));
            bool result = CommandeController.InsertUsingStoredProcedure(insertCommandeRequest, IdentityController.GetUser(this.CurrentUserName()));

            string message = result ? $"Commande inséré avec succès !" : $"Commande échouée !";
            string redirectUrl = $"/commande/";
            string messageTitle = "Ajout d'une commande";
            int sucess = result ? Log.OK_CODE : Log.ERROR_CODE;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object GetModifyCommandeForm(Guid id)
        {
            Commande commande = CommandeController.Select(id).FirstOrDefault();

            CommandeModifyFormViewModel viewModelOfCommande = new CommandeModifyFormViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Commande = commande
            };

            object view = View["CommandeModifyFormView", viewModelOfCommande];
            int success = commande != null ? 1 : 0;
            string message = commande != null ? "Commande non trouvé !" : viewModelOfCommande.Title;

            return this.ResponseObject(view, commande, message, success);
        }

        private object ModifyCommande(CommandeUpdateRequest modifyCommandeValues)
        {

            Commande commande = CommandeController.Select(modifyCommandeValues.Id).FirstOrDefault();

            bool result = CommandeController.Update(modifyCommandeValues, IdentityController.GetUser(this.CurrentUserName()));

            string redirectUrl = $"/commande/";
            string messageTitle = "Modification de la Commande";
            int sucess = result ? 1 : Log.ERROR_CODE;
            string message = result? $"Commande {commande.NumeroCommande} modifié avec succès !" : $"La modification de la commande {commande.NumeroCommande} a échouée !";

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

    }
}