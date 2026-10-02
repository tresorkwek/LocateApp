using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;
using System.Configuration;

namespace LocateApp.ViewModels
{
    public class IdentityModifyFrmViewModel : ViewModel
    {
        public List<Profil> Profil => ProfilController.GetProfil();
        public List<Organe> Entites => OrganeController.SelectEntite();
        public List<Organe> Organigramme { get; set; }
        public string Institution { get; set; }

        public IdentityModifyFrmViewModel(string userName, MessageAlerte messageAlerte = null): base(userName, messageAlerte)
        {
            Institution = ConfigurationManager.AppSettings["Company"];
            Organigramme = OrganeController.Organigramme();
        }
    }
}