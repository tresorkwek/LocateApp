using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class OrganeAddFrmViewModel : ViewModel
    {
        public Organe OrganeParent { get; set; }
        public List<OrganeType> OrganeTypes { get; set; }
        public int OrdreInterne { get; set; }
        public string Link { get; set; }

        public OrganeAddFrmViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/organe/add/";
            OrganeTypes = OrganeTypeController.Select();            
        }
    }
}