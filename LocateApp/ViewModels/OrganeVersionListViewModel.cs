using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class OrganeVersionListViewModel : ViewModel
    {
        public List<OrganeVersion> OrganeVersions { get; set; }
        public string Link { get; set; }

        public OrganeVersionListViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/organe/version/select/";
        }
    }
}