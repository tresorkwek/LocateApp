using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class LocalAddOrModifyFrmViewModel : ViewModel
    {
        public Local Local { get; set; }
        public Organe Organe { get; set; }
        public List<Organe> Organigramme { get; set; }
        public List<LocalType> LocalTypes => LocalTypeController.Select();
        public string Link { get; set; }
        public bool IsSpace { get; set; }

        public LocalAddOrModifyFrmViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/local/add/";
            Organigramme = OrganeController.Organigramme(Identity.IdInstitution);
        }
    }
}