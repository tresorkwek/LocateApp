using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;

namespace LocateApp.ViewModels
{
    public class HopitalViewModel : ViewModel
    {
        public List<Organe> Hopitaux { get; set; }
        public string Link { get; set; }

        public HopitalViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/hopital/modify/";
        }
    }
}