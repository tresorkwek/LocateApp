using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using LocateApp.Controllers;
using LocateApp.Models;

namespace LocateApp.ViewModels
{
    public class ErrorViewModel : ViewModel
    {
        public int Code { get; set; }
        public string Company { get; set; }
        public string CompanyLogo { get; set; }
        public string Titre { get; set; }
        public string HomeUrl => GetHomeUrl();

        public ErrorViewModel(int code, string userName)
        {
            Titre = $"Une erreur {code} s'est produite";
            Code = code;
            Alerte = ConfigurationManager.AppSettings[$"{code}"];
            BodyClass = code < 500 ? "error-page page-404" : "error-page page-500";
            Company = ConfigurationManager.AppSettings["Company"];
            CompanyLogo = ConfigurationManager.AppSettings["CompanyLogo"];
            Identity = IdentityController.GetUser(userName);            
        }

        private string GetHomeUrl()
        {
            return Identity == null ? "/" : Identity.Profil.HomeUrl;
        }

    }
}