using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;

namespace LocateApp.ViewModels
{
    public class ProfilViewModel : ViewModel
    {
        public List<Profil> Profils { get; set; }
        public string Link { get; set; }

        public ProfilViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/profil/modify/";
        }
    }
}