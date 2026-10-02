using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class ClaimViewModel : ViewModel
    {
        public List<Profil> Profils { get; set; }
        public string Link { get; set; }

        public ClaimViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/claim/modify/";
        }
    }
}