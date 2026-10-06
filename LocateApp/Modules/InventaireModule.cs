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
    public class InventaireModule : NancyModule
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(InventaireModule));

        public InventaireModule() : base("/inventaire")
        {
            this.RequiresAuthentication();

            Get("/", _ => this.RunHandler<int?>(GetInventaire, null));            
            Get("/select/{annee}", _ => this.RunHandler<int?>(GetInventaire, (int?)_.annee));
            Get("/encours/", _ => this.RunHandler(GetInventaireEnCours));
            Get("/resultat/", _ => this.RunHandler<int?>(GetResultatInventaire, null));
            Get("/resultat/{annee}", _ => this.RunHandler<int?>(GetResultatInventaire, (int?)_.annee));

            Get("/organe/list/", _ => this.RunHandler<string>(GetInventaireViaOrganigramme, (string)_.codeOrgane));
            Get("/organe/list/{idInstitution_}/", _ => this.RunHandler<string>(GetInventaireViaOrganigramme, (string)_.idInstitution_));
            Get("/organe/{codeOrgane}", _ => this.RunHandler<string>(GetLocalByCodeOrgane, (string)_.codeOrgane));

            Get("/cloturer/", _ => this.RunHandler<int?>(GetInventaireCloturer));
            Get("/cloturer/{annee}", _ => this.RunHandler<int?>(GetInventaireCloturer, (int?)_.annee));
            Get("/details/{annee}", _ => this.RunHandler<int>(GetDetailsInventaire, (int)_.annee));
            Get("/add/", _ => this.RunHandler(GetLancementInventaireForm));
            Get("/modify/{annee}", _ => this.RunHandler<int>(GetModifyInventaireForm, (int)_.annee));           
            Get("/cloturer/{annee}", _ => this.RunHandler<int>(CloturerInventaire, (int)_.annee));           

            Post("/add/", _ => this.RunHandler<LancementInventaireRequest>(LancerInventaire));
            Post("/modify/", _ => this.RunHandler<ModifyInventaireRequest>(ModifierInventaire));
            Post("/details/add/", _ => this.RunHandler<InsertInventaireDetailsRequest>(InsertDetailsInventaire));
            Post("/details/local/", _ => this.RunHandler<InsertInventaireLocalRequest>(InsertDetailsLocalInventaire));
            Post("/nonvu/add/", _ => this.RunHandler<DeclareImmoNonVuRequest>(InsertDetailsInventaireNonVu));
            Post("/identifier/", _ => this.RunHandler<IdentifierImmoRequest>(IdentifierImmo));
            
        }

        private object GetInventaire(int? annee = null)
        {
            List<Inventaire> listOfInventaire = InventaireController.SelectEntete(annee);

            InventaireListViewModel viewModelOfInventaire = new InventaireListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Inventaires = listOfInventaire
            };

            object view = View["InventaireListView", viewModelOfInventaire];
            int success = listOfInventaire.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfInventaire, viewModelOfInventaire.Title, success);
        }

        private object GetResultatInventaire(int? annee = null)
        {
            List<Inventaire> listOfInventaire = InventaireController.SelectEntete(annee);

            InventaireListViewModel viewModelOfInventaire = new InventaireListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Inventaires = listOfInventaire
            };

            object view = View["InventaireResultatListView", viewModelOfInventaire];
            int success = listOfInventaire.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfInventaire, viewModelOfInventaire.Title, success);
        }

        private object GetInventaireViaOrganigramme(string codeOrgane = null)
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
                Organe = organe
            };

            object view = View["LocalListView", viewModelOfLocalList];
            int success = listOfLocaux.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfLocaux, viewModelOfLocalList.Title, success);
        }

        private object GetInventaireCloturer(int? annee)
        {
            List<Inventaire> listOfInventaire = InventaireController.SelectEnteteCloturer(annee);

            InventaireListViewModel viewModelOfInventaire = new InventaireListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Inventaires = listOfInventaire
            };

            object view = View["InventaireListView", viewModelOfInventaire];
            int success = listOfInventaire.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfInventaire, viewModelOfInventaire.Title, success);
        }

        /// <summary>
        /// Détail d'une campagne : bilan par organe ; avec ?organe={code}, lignes inventoriées de cet organe
        /// (une campagne compte plusieurs dizaines de milliers de lignes : on ne les charge jamais toutes).
        /// </summary>
        private object GetDetailsInventaire(int annee)
        {
            string codeOrgane = this.Request.Query["organe"].HasValue ? ((string)this.Request.Query["organe"])?.Trim() : null;

            InventaireDetailsViewModel viewModelOfInventaire = new InventaireDetailsViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Annee = annee,
                Inventaire = InventaireController.SelectEntete(annee).FirstOrDefault()
            };

            object donnees;
            if (string.IsNullOrEmpty(codeOrgane))
            {
                viewModelOfInventaire.Organes = InventaireController.SelectDetailsParOrgane(annee);
                donnees = viewModelOfInventaire.Organes;
            }
            else
            {
                viewModelOfInventaire.Organe = OrganeController.SelectById(codeOrgane);
                viewModelOfInventaire.Lignes = InventaireController.SelectDetailsByOrgane(annee, codeOrgane);
                donnees = viewModelOfInventaire.Lignes;
            }

            object view = View["InventaireDetailsListView", viewModelOfInventaire];
            int success = viewModelOfInventaire.Inventaire != null ? 1 : 0;

            return this.ResponseObject(view, donnees, viewModelOfInventaire.Title, success);
        }

        private object GetInventaireEnCours()
        {
            int anneeEnCours = InventaireController.SelectAnneeEnCours();
            Inventaire inventaire = InventaireController.SelectEntete(anneeEnCours).FirstOrDefault();

            InventaireViewModel viewModelOfObservation = new InventaireViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Inventaire = inventaire
            };

            object view = View["InventaireView", viewModelOfObservation];
            int success = anneeEnCours > 0 ? 1 : 0;

            return this.ResponseObject(view, inventaire, viewModelOfObservation.Title, success);
        }


        private object GetLancementInventaireForm()
        {
            ViewModel viewModel = new ViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString()
            };

            object view = View["InventaireAddFrmView", viewModel];
            int success = 1;

            return this.ResponseObject(view, null, viewModel.Title, success);
        }

        private object GetModifyInventaireForm(int annee)
        {
            Inventaire inventaire = InventaireController.SelectEntete((int)annee).FirstOrDefault();

            if (inventaire == null)
            {
                string message = $"L'inventaire de  l'année {annee} n'a pas encore été lancé !";
                string redirectUrl = $"/inventaire/";
                string messageTitle = "Modification d'un inventaire";
                int sucess = 0;

                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            InventaireViewModel viewModel = new InventaireViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Inventaire = inventaire
            };

            object view = View["InventaireModifyFrmView", viewModel];
            int success = 1;

            return this.ResponseObject(view, null, viewModel.Title, success);
        }

        private object InsertDetailsInventaire(InsertInventaireDetailsRequest InsertInventaireDetailsValues)
        {

            bool result = InventaireController.InsertDetails(InsertInventaireDetailsValues, IdentityController.GetUser(this.CurrentUserName()));

            Immo immo = ImmoController.SelectById(InsertInventaireDetailsValues.IdImmo);

            string message = result ? $"Inventaire de  {immo.Designation} de l'année {InsertInventaireDetailsValues.Annee} effectué avec succès !" : $"L'inventaire de {immo.Designation} pour l'anné {InsertInventaireDetailsValues.Annee} a échoué !";
            string redirectUrl = $"/inventaire/select/{InsertInventaireDetailsValues.Annee}";
            string messageTitle = "Inventaire d'un bien";
            int sucess = result ? 1 : 0;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);

        }

        private object InsertDetailsLocalInventaire(InsertInventaireLocalRequest InsertInventaireDetailsLocalValues)
        {

            bool result = InventaireController.InsertDetailsLocal(InsertInventaireDetailsLocalValues, IdentityController.GetUser(this.CurrentUserName()));

            Local local = LocalController.SelectById(InsertInventaireDetailsLocalValues.IdLocal).FirstOrDefault();

            string message = result ? $"Inventaire du local : {local.Designation} pour l'année {InsertInventaireDetailsLocalValues.Annee} effectué avec succès !" : $"L'inventaire du local : {local.Designation} pour l'anné {InsertInventaireDetailsLocalValues.Annee} a échoué !";
            string redirectUrl = $"/immo/local/{InsertInventaireDetailsLocalValues.IdLocal}";
            string messageTitle = "Inventaire d'un local";
            int sucess = result ? 1 : 0;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object InsertDetailsInventaireNonVu(DeclareImmoNonVuRequest DeclareImmoNonVuValues)
        {

            bool result = InventaireController.InsertDetailsNonVu(DeclareImmoNonVuValues, IdentityController.GetUser(this.CurrentUserName()));

            Immo immo = ImmoController.SelectById(DeclareImmoNonVuValues.IdImmo);

            string message = result ? $"Inventaire de  {immo.Designation} de l'année {immo.LastAnneeComptable} effectué avec succès !" : $"L'inventaire de {immo.Designation} pour l'anné {immo.LastAnneeComptable} a échoué !";
            string redirectUrl = $"/inventaire/select/{immo.LastAnneeComptable}";
            string messageTitle = "Inventaire d'un bien";
            int sucess = result ? 1 : 0;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);

        }

        private object IdentifierImmo(IdentifierImmoRequest IdentifierImmoValues)
        {

            bool result = InventaireController.IdentifierImmo(IdentifierImmoValues, IdentityController.GetUser(this.CurrentUserName()));

            Immo immo = ImmoController.SelectById(IdentifierImmoValues.IdImmo);

            string message = result ? $"{immo.Designation} identifié avec succès en l'année {immo.LastAnneeComptable} " : $"L'identification de {immo.Designation} pour l'année {immo.LastAnneeComptable} a échoué !";
            string redirectUrl = $"/immo/select/{immo.CodeADM}";
            string messageTitle = "Identification d'un bien";
            int sucess = result ? 1 : 0;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);

        }

        private object LancerInventaire(LancementInventaireRequest lancementInventaireValues)
        {
            bool result = false;
            int? inventaireEnCours = InventaireController.SelectAnneeEnCours();

            string message = "Impossible de lancer un nouvel inventaire avant de clôturer le dernier";

            if (inventaireEnCours == null || inventaireEnCours == 0)
            {
                result = InventaireController.InsertEntete(lancementInventaireValues, IdentityController.GetUser(this.CurrentUserName()));
                message = result ? $"Inventaire de l'année {lancementInventaireValues.Annee} lancé avec succès !" : $"Le lancement de l'inventaire de l'anné {lancementInventaireValues.Annee} a échoué !";
            }
             
            string redirectUrl = $"/inventaire/";
            string messageTitle = "Lancement d'un inventaire";
            int sucess = result ? 1 : 0;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);

        }

        private object ModifierInventaire(ModifyInventaireRequest modifierInventaireValues)
        {
            bool result = false;

            Inventaire inventaire = InventaireController.SelectEntete((int)modifierInventaireValues.Annee).FirstOrDefault();

            string message = "Impossible de modifier un inventaire déjà clôturer";

            if (inventaire != null && string.IsNullOrEmpty(inventaire.UserCloture))
            {
                result = InventaireController.UpdateEntete(modifierInventaireValues, IdentityController.GetUser(this.CurrentUserName()));
                message = result ? $"Inventaire de l'année {modifierInventaireValues.Annee} lancé avec succès !" : $"Le lancement de l'inventaire de l'anné {modifierInventaireValues.Annee} a échoué !";
            }

            string redirectUrl = $"/inventaire/";
            string messageTitle = "Modification d'un inventaire";
            int sucess = result ? 1 : 0;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);

        }

        private object CloturerInventaire(int annee)
        {
            bool result = false;

            Inventaire inventaire = InventaireController.SelectEntete(annee).FirstOrDefault();

            string message = "Impossible de clôturer un inventaire déjà clôturer";

            //if (inventaire != null && inventaire.ReadyToClose())
            if (inventaire != null)
            {
                if (string.IsNullOrEmpty(inventaire.UserCloture))
                {
                    result = InventaireController.Cloture(annee, IdentityController.GetUser(this.CurrentUserName()));
                    message = result ? $"Inventaire de l'année {annee} clôturé avec succès !" : $"La cloture de l'inventaire de l'anné {annee} a échouée !";
                }
            }
            else
            {
                message = inventaire != null ? "Impossible de clôturer l'inventaire tant que tous les biens ne sont pas inventoriés" : "Impossible de clôturer un inventaire pour une année non ouverte";
            }            

            string redirectUrl = $"/inventaire/";
            string messageTitle = "Clôture de l'inventaire";
            int sucess = result ? 1 : 0;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

    }
}