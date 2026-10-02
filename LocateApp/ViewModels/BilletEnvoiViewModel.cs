using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;
using System.Configuration;

namespace LocateApp.ViewModels
{
    public class BilletEnvoiViewModel : ViewModel
    {
        public BilletEnvoi BilletEnvoi { get; set; }
        public bool CanApprove => GetApproveStatus();
        public bool CanModify => GetModifyStatus();
        public string Link { get; set; }
        public string PrintLink { get; set; }
        public bool IsMyProfil { get; set; }

        public BilletEnvoiViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            BodyClass = "white";
            BodyStyle = "margin-left:0px;";
            Link = "/billetenvoi/modify/";

        }
        private bool GetApproveStatus()
        {
            return Utilities.Utilities.CheckClaimStatus(Identity, "GetBilletenvoiApproveValues");
        }
        private bool GetModifyStatus() 
        {
            return Utilities.Utilities.CheckClaimStatus(Identity, "GetBilletenvoiModifyId");
        }

       

    }
}