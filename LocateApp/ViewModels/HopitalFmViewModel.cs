using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class HopitalFmViewModel : ViewModel
    {
        public Organe Hopital { get; set; }
        public List<ServiceMedical> ServiceMedical => ServiceMedicalController.GetService();
        public string Link { get; set; }

        public HopitalFmViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            BodyClass = "white";
            Link = "/hopital/add/";
        }

    }
}