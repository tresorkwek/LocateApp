using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class ImmoViewModel : ViewModel
    {
        public Immo Immo { get; set; }
        public string Link { get; set; }
        public Agent Responsable { get; set; }
        public Organe OrganeResponsable { get; set; }

        public ImmoViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/immo/modify/";
        }
    }
}