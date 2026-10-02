using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;

namespace LocateApp.ViewModels
{
    public class OrganeTypeModifyFrmViewModel : ViewModel
    {
        public OrganeType OrganeType { get; set; }
        public string Link { get; set; }

        public OrganeTypeModifyFrmViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/organe/type/modify/";
        }
    }
}