using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using LocateApp.Models;

namespace LocateApp.ViewModels
{
    public class LoginViewModel: ViewModel
    {
        public string Icone { get; set; }
        public string Titre { get; set; }
        public LoginViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            string appName = ConfigurationManager.AppSettings["Logiciel"];
            string version = ConfigurationManager.AppSettings["Version"];

            Titre = $"L'accès à {appName} {version} requiert une authentification";
            Alerte = "Authentifiez-vous SVP !";
            Icone = "lock_open";
            TypeTitle = 3;
        }

    }
}