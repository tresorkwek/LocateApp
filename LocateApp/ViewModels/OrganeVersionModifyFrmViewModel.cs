using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;

namespace LocateApp.ViewModels
{
    public class OrganeVersionModifyFrmViewModel : ViewModel
    {
        public OrganeVersion OrganeVersion { get; set; }
        public string Link { get; set; }

        public OrganeVersionModifyFrmViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/organe/version/modify/";
        }
    }
}