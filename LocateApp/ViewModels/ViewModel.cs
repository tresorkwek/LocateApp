
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;
using LocateApp.Utilities;
using System.Configuration;

namespace LocateApp.ViewModels
{
    public class ViewModel
    {
        public Identity Identity { get; set; }        
        public long TypeTitle { get; set; }
        public string Alerte { get; set; }
        public string AlerteTitle { get; set; }
        public string CurrentClaim { get; set; }
        public string BodyClass { get; set; }
        public string BodyStyle { get; set; }
        public bool ShowOnlyRenderBody { get; set; }
        public string OnLoad { get; set; }
        public Menu MenuData { get; set; }
        public bool CanSearch => GetSearchStatus();
        public string Title => GetTitle();
        public string Description => GetDescription();
        public string AlertColor => GettAlertColor();
        public string TypeAlert => GetTypeAlert();
        public string AppName => GetAppName();
        public string AppVersion => ConfigurationManager.AppSettings["Version"];
        public string Company => ConfigurationManager.AppSettings["Company"];
        public string PhotoPath => GetPhotoPath();

        public ViewModel()
        {
            
        }

        public ViewModel(string userName,MessageAlerte messageAlerte)
        {

            if(!string.IsNullOrEmpty(userName))
                Identity = IdentityController.GetUser(userName);

            if(messageAlerte != null)
            {
                Alerte = messageAlerte.Alerte;
                TypeTitle = messageAlerte.TypeTitle;
                AlerteTitle = messageAlerte.AlerteTitle;
            }
            else
            {
                TypeTitle = 4;
            }

            ShowOnlyRenderBody = false;


        }

        public string GettAlertColor()
        {
            string color;

            switch (TypeTitle)
            {
                case 0:
                    color = "red";
                    break;
                case 1:
                    color = "green";
                    break;
                case 2:
                    color = "orange";
                    break;
                case 3:
                    color = "blue darken-4";
                    break;

                default:
                    color = "purple";
                    break;
            }

            return color;
        }

        public string GetTypeAlert()
        {
            string typeAlert;

            switch (TypeTitle)
            {
                case 0:
                    typeAlert = "error";
                    break;
                case 1:
                    typeAlert = "success";
                    break;
                case 2:
                    typeAlert = "warning";
                    break;
                case 3:
                    typeAlert = "info";
                    break;

                default:
                    typeAlert = null;
                    break;
            }

            return typeAlert;
        }

        public string GetTitle()
        {
            string response = null;

            if(MenuData != null)
            {
                response = MenuData?.Titre;
            }

            return response;
        }

        public string GetDescription()
        {
            string response = null;

            if (MenuData != null)
            {
                response = MenuData?.Commentaire; 
            }

            return response;
        }

        public bool GetSearchStatus()
        {
            return Utilities.Utilities.CheckClaimStatus(Identity, "GetPatientSelect");
        }

        private string GetAppName()
        {
            return ConfigurationManager.AppSettings["Logiciel"];
        }

        private string GetPhotoPath()
        {
            return Utilities.Utilities.GetPhotoAgentPath();
        }

    }
}