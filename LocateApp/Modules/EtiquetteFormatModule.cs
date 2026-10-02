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
using ClosedXML.Excel;
using System.Text;
using System.IO;
using System.Globalization;

namespace LocateApp.Modules
{
    public class EtiquetteFormatModule : NancyModule
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(EtiquetteFormatModule));

        public EtiquetteFormatModule() : base("/etiquette/format")
        {
            this.RequiresAuthentication();

            Get("/select/", _ => this.RunHandler<int?>(GetEtiquetteFormat,null));
            Get("/Select/{id}", _ => this.RunHandler<int?>(GetEtiquetteFormat, (int)_.id));
            Get("/modify/{id}", _ => this.RunHandler<int>(GetModifyEtiquetteFormatForm, (int)_.id));
            Get("/add/", _ => this.RunHandler(GetAddEtiquetteFormatForm));

            Post("/add/", _ => this.RunHandler<EtiquetteFormatInsertRequest>(AddEtiquetteFormat));
            Post("/modify/", _ => this.RunHandler<EtiquetteFormatModifytRequest>(ModifyEtiquetteFormat));
        }

        private object GetEtiquetteFormat(int? id)
        {
            List<EtiquetteFormat> listOfEtiquetteFormat = EtiquetteFormatController.Select(id,true);

            EtiquetteFormatListViewModel viewModelOfEtiquetteFormat = new EtiquetteFormatListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                EtiquetteFormats = listOfEtiquetteFormat
            };

            object view = View["EtiquetteFormatListView", viewModelOfEtiquetteFormat];
            int success = listOfEtiquetteFormat.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfEtiquetteFormat, viewModelOfEtiquetteFormat.Title, success);
        }

        private object GetAddEtiquetteFormatForm()
        {

            ViewModel viewModelOCategorie = new ViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString()
            };

            object view = View["EtiquetteFormatAddFrmView", viewModelOCategorie];
            int success = 1;

            return this.ResponseObject(view, null, viewModelOCategorie.Title, success);
        }

        private object AddEtiquetteFormat(EtiquetteFormatInsertRequest insertEtiquetteFormatValues)
        {
            bool result = EtiquetteFormatController.Insert(insertEtiquetteFormatValues, IdentityController.GetUser(this.CurrentUserName()));

            string message = result ? $"Format de l'étiquette {insertEtiquetteFormatValues.Nom} crée avec succès !" : $"La création du format de l'étiquette {insertEtiquetteFormatValues.Nom} a échouée !";
            string redirectUrl = $"/etiquette/format/select/";
            string messageTitle = "Création d'un format d'étiquette";
            int sucess = result ? 1 : 0;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object GetModifyEtiquetteFormatForm(int id)
        {
            EtiquetteFormat formatEtiquette = EtiquetteFormatController.Select(id, true).FirstOrDefault();

            EtiquetteFormatModifyFrmViewModel viewModelOFamille = new EtiquetteFormatModifyFrmViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                EtiquetteFormat = formatEtiquette
            };

            object view = View["EtiquetteFormatModifyFrmView", viewModelOFamille];
            int success = 1;

            return this.ResponseObject(view, null, viewModelOFamille.Title, success);
        }

        private object ModifyEtiquetteFormat(EtiquetteFormatModifytRequest modifyEtiquetteFormatValues)
        {
            bool result = EtiquetteFormatController.Update(modifyEtiquetteFormatValues, IdentityController.GetUser(this.CurrentUserName()));

            string message = result ? $"Le format de l'étiquette {modifyEtiquetteFormatValues.Nom}  a été modifié avec succès !" : $"La modification du format de l'étiquette {modifyEtiquetteFormatValues.Nom} a échouée !";
            string redirectUrl = $"/etiquette/format/select/";
            string messageTitle = "Modification d'un format d'étiquette";
            int sucess = result ? 1 : 0;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }
        
    }
}