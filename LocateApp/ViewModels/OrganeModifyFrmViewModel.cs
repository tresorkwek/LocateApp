using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class OrganeModifyFrmViewModel : ViewModel
    {
        public Organe Organe { get; set; }
        public List<OrganeType> OrganeTypes { get; set; }
        public List<OrganeVersion> OrganeVersions { get; set; }
        public List<Organe> Organigramme { get; set; }
        public string Link { get; set; }

        public OrganeModifyFrmViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/organe/modify/";
            OrganeTypes = OrganeTypeController.Select();
            Organigramme = OrganeController.Organigramme();
            OrganeVersions = OrganeVersionController.Select();
        }
    }
}