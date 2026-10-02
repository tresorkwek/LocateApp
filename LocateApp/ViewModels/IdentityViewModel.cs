using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;

namespace LocateApp.ViewModels
{
    public class IdentityViewModel : ViewModel
    {
        public List<Identity> Identities { get; set; }
        public string Link { get; set; }
        public string LinkForAgent { get; set; }
        public string Action { get; set; }
        public string ActionLabel { get; set; }
        public string ActionName { get; set; }
        public string ActionIcone { get; set; }

        public IdentityViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/utilisateur/modify/";
            LinkForAgent = "/patient/select/";
            Action = "/utilisateur/disable/";
            ActionName = "Désactivation";
            ActionLabel = "désactiver";
            ActionIcone = "person_remove";
        }
    }
}