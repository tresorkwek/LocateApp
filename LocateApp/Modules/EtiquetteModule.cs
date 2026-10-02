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
    public class EtiquetteModule : NancyModule
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(EtiquetteModule));

        public EtiquetteModule() : base("/etiquette")
        {
            this.RequiresAuthentication();
           
            Get("/select/{idCommande}", _ => this.RunHandler<Guid>(ShowEtiquette, (Guid)_.idCommande));
            Get("/commande/", _ => this.RunHandler<Guid?>(GetCommande,null));
            Get("/commande/{idCommande}", _ => this.RunHandler<Guid?>(GetCommande, (Guid?)_.idCommande));
            Get("/impression/{idCommande}", _ => this.RunHandler<Guid>(GenererEtiquette, (Guid)_.idCommande));
            Get("/download/{idCommande}", _ => this.RunHandler<Guid>(DownloadEtiquette, (Guid)_.idCommande));
            Get("/package/{idCommande}", _ => this.RunHandler<Guid>(PackagerEtiquette, (Guid)_.idCommande));
            Get("/genereted/", _ => this.RunHandler<Guid?>(GetEtiquette, null));
            Get("/printed/", _ => this.RunHandler<Guid?>(GetEtiquettePrint, null));
            Get("/genereted/{idCommande}", _ => this.RunHandler<Guid?>(GetEtiquette, null)); 
            Get("/printed/{idCommande}", _ => this.RunHandler<Guid?>(GetEtiquettePrint, null));
            Get("/unused/", _ => this.RunHandler(ShowEtiquetteUnusedPrinted));

        }


        private object ShowEtiquetteUnusedPrinted()
        {
            List<Etiquette> listOfEtiquette = EtiquetteController.SelectUnusedPrinted();

            if (listOfEtiquette.Count == 0)
            {
                string message = "Aucune étiquette générée pour cette commande !";
                string redirectUrl = "/etiquette/commande/";
                string messageTitle = "Afficheage des etiquettes";
                int sucess = Log.ERROR_CODE;

                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }


            EtiquetteViewModel viewModelOfEtiquette = new EtiquetteViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                ShowOnlyRenderBody = true,
                Etiquettes = listOfEtiquette
            };

            object view = View["EtiquetteView", viewModelOfEtiquette];
            int success = listOfEtiquette.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfEtiquette, viewModelOfEtiquette.Title, success);
        }
        private object ShowEtiquette(Guid idCommande)
        {
            List<Etiquette> listOfEtiquette = EtiquetteController.Select(idCommande);
            Commande commande = CommandeController.Select(idCommande).FirstOrDefault();

            if (listOfEtiquette.Count == 0)
            {
                string message = "Aucune étiquette générée pour cette commande !";
                string redirectUrl = "/etiquette/commande/";
                string messageTitle = "Afficheage des etiquettes";
                int sucess = Log.ERROR_CODE;

                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }


            EtiquetteViewModel viewModelOfEtiquette = new EtiquetteViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                ShowOnlyRenderBody = true,
                Etiquettes = listOfEtiquette,
                Commande = commande
            };

            object view = View["EtiquetteView", viewModelOfEtiquette];
            int success = listOfEtiquette.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfEtiquette, viewModelOfEtiquette.Title, success);
        }

        private object DownloadEtiquette(Guid idCommande)
        {
            List<Etiquette> listOfEtiquette = EtiquetteController.Select(idCommande);
            Commande commande = CommandeController.Select(idCommande).FirstOrDefault();

            if (listOfEtiquette.Count == 0)
            {
                string message = "Aucune étiquette générée pour cette commande !";
                string redirectUrl = "/etiquette/commande/";
                string messageTitle = "Afficheage des etiquettes";
                int sucess = Log.ERROR_CODE;

                return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
            }

            var etiquettesList = listOfEtiquette.Select(e => new QrCodeEtiquette { QrCode = e.QrCode.ToString(), Id = e.Id.ToString() });

            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms, Encoding.UTF8);

            Utilities.Utilities.ToTextWriter(etiquettesList, sw);
            sw.Flush();

            Stream stream = ms;
            stream.Position = 0;

            return Response.FromStream(stream, "text/html;charset=utf-8")
                .WithHeader("Content-disposition", $"attachment;filename=Etiquette_CMD-{commande.NumeroCommande}.csv");

        }

        private object GetCommande(Guid? idCommande = null)
        {
            List<Commande> listOfCommande = CommandeController.Select(idCommande);

            CommandeViewModel viewModelOCommande = new CommandeViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Commandes = listOfCommande,
                Link = "/etiquette/impression/"
            };                      

            object view = View["EtiquetteCommandeListView", viewModelOCommande];
            int success = listOfCommande.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfCommande, viewModelOCommande.Title, success);
        }

        private object GetEtiquette(Guid? idCommande = null)
        {
            List<Commande> listOfCommande = CommandeController.Select(idCommande);

            CommandeViewModel viewModelOCommande = new CommandeViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Commandes = listOfCommande,
                Link = "/etiquette/select/"
            };

            bool test;
            foreach (var commande in listOfCommande)
            {
                test = commande.IsValidate();
            }

            object view = View["EtiquetteListView", viewModelOCommande];
            int success = listOfCommande.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfCommande, viewModelOCommande.Title, success);
        }

        private object GetEtiquettePrint(Guid? idCommande = null)
        {
            List<Commande> listOfCommande = CommandeController.SelectPrinted(idCommande);

            CommandeViewModel viewModelOCommande = new CommandeViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Commandes = listOfCommande,
                Link = "/etiquette/select/"
            };

            object view = View["QrCodeListView", viewModelOCommande];
            int success = listOfCommande.Count > 0 ? 1 : 0;

            return this.ResponseObject(view, listOfCommande, viewModelOCommande.Title, success);
        }


        private object GenererEtiquette(Guid idCommande)
        {
            Commande commande = CommandeController.Select(idCommande).FirstOrDefault();

            bool result = EtiquetteController.InsertUsingStoredProcedure(commande, IdentityController.GetUser(this.CurrentUserName()));

            string message = result ? $"Etiquetes générées avec succès !" : $"La génération des étiquetes a échouée !";
            string redirectUrl = result ? $"/etiquette/genereted/" : "/etiquette/commande/";
            string messageTitle = "Génération des etiquettes";
            int sucess = result ? Log.OK_CODE : Log.ERROR_CODE;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

        private object PackagerEtiquette(Guid idCommande)
        {
            Commande commande = CommandeController.Select(idCommande).FirstOrDefault();

            bool result = EtiquetteController.Print(commande, IdentityController.GetUser(this.CurrentUserName()));

            string message = result ? $"Commande {commande.NumeroCommande} empaquetée avec succès !" : $"L'empaquetage de la commande a échouée !";
            string redirectUrl = $"/etiquette/genereted/";
            string messageTitle = "Empaquetage d'une commande";
            int sucess = result ? 1 : Log.ERROR_CODE;

            return this.RedirectUrl(redirectUrl, message, sucess, messageTitle);
        }

    }
}