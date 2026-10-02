using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class OrganeTypeListViewModel : ViewModel
    {
        public List<OrganeType> OrganeTypes { get; set; }
        public string Link { get; set; }

        public OrganeTypeListViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/organe/type/select/";
        }
    }
}