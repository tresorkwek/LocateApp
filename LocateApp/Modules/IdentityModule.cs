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
using System.Configuration;
using System.IO;

namespace LocateApp.Modules
{
    public class IdentityModule : NancyModule
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(IdentityModule));

        public IdentityModule() : base("/utilisateur")
        {
            this.RequiresAuthentication();

            Get("/", _ => this.RunHandler<string>(GetUser, null));
            Get("/select/", _ => this.RunHandler<string>(GetUser, null));
            Get("/select/{userName}", _ => this.RunHandler<string>(GetUser, (string)_.userName));
            Get("/import/", _ => this.RunHandler<string>(GetImportUserForm, null));
            Get("/import/{matricule}", _ => this.RunHandler<string>(GetImportUserForm, (string)_.matricule));
            Get("/add/", _ => this.RunHandler(GetAddUserForm));
            Get("/modify/{matricule}", _ => this.RunHandler<string>(GetModifyUserForm, (string)_.matricule));
            Get("/disable/", _ => this.RunHandler(GetDisableUser));
            Get("/disable/{matricule}", _ => this.RunHandler<string,bool>(ChangeUseStatus, (string)_.matricule,true));
            Get("/enable/{matricule}", _ => this.RunHandler<string,bool>(ChangeUseStatus, (string)_.matricule,false));
            Get("/reset/{matricule}", _ => this.RunHandler<string>(Reset, (string)_.matricule));

            Post("/import/", _ => this.RunHandler<ImportIdentityRequest>(ImportUser));
            Post("/add/", _ => this.RunHandler<IdentityInsertRequest>(InsertIdentity)); 
            Post("/modify/", _ => this.RunHandler<IdentityUpdateRequest>(ModifyIdentity));
        }        

        private object GetUser(string userName = null)
        {
            List<Identity> listOfUsers = userName == null ? IdentityController.GetIdentity() : new List<Identity> { IdentityController.GetUser(userName) };

            IdentityViewModel viewModelOfMenu = new IdentityViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Identities = listOfUsers
            };

            object view = View["IdentityView", viewModelOfMenu];
            int success = listOfUsers.Count > 0 ? 1 : 0;


            return this.ResponseObject(view, listOfUsers, viewModelOfMenu.Title, success);
        }


        private object GetDisableUser()
        {
            List<Identity> listOfUsers = IdentityController.GetDisableIdentity() ;

            IdentityViewModel viewModelOfMenu = new IdentityViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Identities = listOfUsers,
                Action = "/utilisateur/enable/",
                ActionName = "Activation",
                ActionLabel = "activer",
                ActionIcone = "person_add"                
            };

            object view = View["IdentityView", viewModelOfMenu];
            int success = listOfUsers.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfUsers, viewModelOfMenu.Title, success);
        }

        private object ChangeUseStatus(string matricule,bool desable)
        {

            bool result = IdentityController.ChangeIdentityStatus(matricule, desable, IdentityController.GetUser(this.CurrentUserName()));


            Identity user = IdentityController.GetUserIdentity(matricule);
            string nomUser = $"{user.Nom.Trim()} {user.Postnom.Trim()} {user.Prenom.Trim()}";
            string message;

            if (result)
            {
                message = desable ? $"La désactivation de {nomUser} a réussie avec succès !" : $"L'activation de {nomUser} a réussie avec succès !";
            }
            else 
            {
                message = desable ? $"La désactivation de {nomUser} a échouée !" : $"L'activation de {nomUser} a échouée !";
            }

            string redirectUrl = desable ? $"/utilisateur/" : "/utilisateur/disable/";
            string messageTitle = desable? $"Désactivation d'un utilisateur" : $"Activation d'un utilisateur";
            int sucess = result ? 1 : Log.ERROR_CODE;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object GetImportUserForm(string matricule = null)
        {
            List<Agent> listOfAgent = AgentController.SelectAgentActifNotUser(matricule);

            IdentityImportViewModel viewModel = new IdentityImportViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = "GetUtilisateur",
                Agents = listOfAgent
            };

            object view = View["IdentityImportView", viewModel];
            int success = 3;

            return this.ResponseObject(view, null, viewModel.Title, success);
        }


        private object GetAddUserForm()
        {
            IdentityAddFrmViewModel viewModelOfUser = new IdentityAddFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = "GetUtilisateur"
            };

            object view = View["IdentityAddFrmView", viewModelOfUser];
            int success = 1;

            return this.ResponseObject(view, null, viewModelOfUser.Title, success);
        }

        private object ImportUser(ImportIdentityRequest importIdentityRequest)
        {               

            bool result = IdentityController.ImportIdentities(importIdentityRequest, IdentityController.GetUser(this.CurrentUserName()), out List<Identity> identities);

            string identitiesList = "";
            string identityName; 

            foreach (var identity in identities)
            {
                
                identityName = $"{identity.Nom} {identity.Postnom} {identity.Prenom}";
                identitiesList += $"<div class='chip'> <img src='{Utilities.Utilities.GetPhotoAgentPath() + identity.Photo}' alt = '{identityName}' /> {identityName} </div >";
               
            }

            string redirectUrl = $"/utilisateur/";
            string messageTitle = "Importation des Utilisateurs";
            int sucess = result ? Log.SUCCESS_CODE : Log.ERROR_CODE;
            string action = identities.Count > 1 ? "importés" : "importé"; 

            string message = result ? $"{identitiesList} <br/> {action} avec succès !" : $"L'importation de <br/> {identitiesList} <br/> a échoué !";

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object GetModifyUserForm(string matricule)
        {
            Identity user = IdentityController.GetUserIdentity(matricule);

            IdentityModifyFrmViewModel viewModelOfUser = new IdentityModifyFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Identity = user
            };

            object view = View["IdentityModifyFrmView", viewModelOfUser];
            int success = 1;

            return this.ResponseObject(view, user, viewModelOfUser.Title, success);
        }

        private object InsertIdentity(IdentityInsertRequest insertIdentityRequest)
        {
            bool result = IdentityController.InsertIdentity(insertIdentityRequest, IdentityController.GetUser(this.CurrentUserName()));

            var file = this.Request.Files.FirstOrDefault();            

            if (result && insertIdentityRequest.IsExternalUser && file != null) 
            {
                DefaultRootPathProvider pathProvider = new DefaultRootPathProvider();
                var filename = Path.Combine(pathProvider.GetRootPath(), "Content/images/photos", insertIdentityRequest.UserName + "00.jpg"); // nom lu par Identity.Photo
                
                using (var fileStream = new FileStream(filename, FileMode.Create))
                {
                    file.Value.CopyTo(fileStream);
                }                   
            }

            Identity identity = IdentityController.GetUser(insertIdentityRequest.UserName);

            string identityName = $"<div class='chip'> {identity.Nom} {identity.Postnom} {identity.Prenom} </div>";
            string identityPhoto = $"<img src ='/Content/images/photos/{identity.Photo}' width='130px'  class='circle mailbox-profile'>";

            string redirectUrl = $"/utilisateur/";
            string messageTitle = "Insertion de l'utilisateur";
            int sucess = result ? Log.SUCCESS_CODE : Log.ERROR_CODE;
            string message = result ? $"{identityPhoto} <br/> {identityName} <br/> inséré avec succès !" : $"La création de l'utilisateur <div class='chip'> {insertIdentityRequest.Nom} {insertIdentityRequest.Postnom} {insertIdentityRequest.Prenom} </div> a échouée !";

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object ModifyIdentity(IdentityUpdateRequest modifyIdentityRequest)
        {
            bool result = IdentityController.ModifyIdentity(modifyIdentityRequest, IdentityController.GetUser(this.CurrentUserName()));

            var file = this.Request.Files.FirstOrDefault();

            if (result && modifyIdentityRequest.IsExternalUser && file != null)
            {
                DefaultRootPathProvider pathProvider = new DefaultRootPathProvider();
                var filename = Path.Combine(pathProvider.GetRootPath(), "Content/images/photos", modifyIdentityRequest.UserName + "00.jpg"); // nom lu par Identity.Photo

                using (var fileStream = new FileStream(filename, FileMode.Create))
                {
                    file.Value.CopyTo(fileStream);
                }
            }

            string redirectUrl = $"/utilisateur/";
            string messageTitle = "Modification de l'utilisateur";
            int sucess = result ? Log.SUCCESS_CODE : Log.ERROR_CODE;
            string message = result ? $"La modification de {modifyIdentityRequest.Nom} {modifyIdentityRequest.Postnom} {modifyIdentityRequest.Prenom} s'est effectuée avec succès !" : $"La modification de {modifyIdentityRequest.Nom} {modifyIdentityRequest.Postnom} {modifyIdentityRequest.Prenom} a échouée !";

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object Reset(string matricule)
        {
            string message = $"Le compte utilisateur {matricule} n'existe pas !";
            string messageTitle = "Réinitionalisation du mot de passe";
            int success = Log.ERROR_CODE;
            string logicielName = ConfigurationManager.AppSettings["Logiciel"];
            string domaineName = ConfigurationManager.AppSettings["DomaineName"];

            Identity identity = IdentityController.GetUser(matricule);

            if (identity != null && identity.IsExternalUser)
            {
                int responseValue = IdentityController.ResetPassword(identity.IdUser, IdentityController.GetUser(this.CurrentUserName()));

                if (responseValue > 0)
                {
                    message = $"Le compte de l'utilisateur {identity.UserName} a été réinitionalité avec succès !";
                    success = Log.SUCCESS_CODE;
                    Log.Info(identity, message);
                }
                else
                {
                    message = $"La réinitionalisation du compte {identity.UserName} a échouée !";
                    Log.Error(identity, message);
                }

            }
            else
            {
                message = identity != null ? $"Impossible de réinitialiser un mot de passe {domaineName} dans {logicielName}" : $"{message.Substring(0, message.Length - 1)} lors de la {messageTitle}";
                Log.Error(identity, message);
            }

            return this.RedirectUrl("/utilisateur/", message, success, messageTitle);
        }

    }
}