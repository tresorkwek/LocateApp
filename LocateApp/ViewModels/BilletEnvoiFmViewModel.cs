using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class BilletEnvoiFmViewModel : ViewModel
    {
        public Agent Patient { get; set; }
        public bool SelectPublicHospitalOnly { get; set; }
        public string Link { get; set; }
        public List<Organe> Hopital => SelectHospitals();

        public BilletEnvoiFmViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            BodyClass = "white";
            Link = "/billetenvoi/add/";
            SelectPublicHospitalOnly = false;
        }

        private List<Organe> SelectHospitals()
        {
            return OrganeController.Select();
        }

    }
}