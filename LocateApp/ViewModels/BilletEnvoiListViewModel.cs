using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class BilletEnvoiListViewModel : ViewModel
    {
        public List<BilletEnvoi> BilletEnvoi { get; set; }
        public bool CanApprove => GetApproveStatus();
        public string Link { get; set; }
        

        public BilletEnvoiListViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            BodyClass = "white";
            Link = "/billetenvoi/select/";            
        }

        private bool GetApproveStatus() 
        {
            return Utilities.Utilities.CheckClaimStatus(Identity, "GetBilletenvoiApproveValues");
        }

    }
}