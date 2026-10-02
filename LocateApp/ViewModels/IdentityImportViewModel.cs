using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Controllers;
using LocateApp.Models;

namespace LocateApp.ViewModels
{
    public class IdentityImportViewModel : ViewModel
    {
        public List<Agent> Patients { get; set; }
        public List<Profil> Profils => ProfilController.GetProfil();
        public string Link { get; set; }

        public IdentityImportViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/patient/select/";
            
        }
    }
}