using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class ClaimAddFrmViewModel : ViewModel
    {
        public List<Profil> Profils => ProfilController.GetProfilWithoutClaims();
        public List<GroupMenu> GroupMenus => GroupMenuController.GetAllGroupMenu();
        public string Link { get; set; }

        public ClaimAddFrmViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/claim/modify/";

        }
    }
}