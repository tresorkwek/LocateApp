using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Controllers;
using LocateApp.Models;

namespace LocateApp.ViewModels
{
    public class IdentityAddFrmViewModel : ViewModel 
    {
        public List<Profil> Profil => ProfilController.GetProfil();
        public List<Organe> Organes => OrganeController.Select();

        public IdentityAddFrmViewModel(string userName, MessageAlerte messageAlerte = null): base(userName, messageAlerte)
        {

        }
    }
}