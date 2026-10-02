using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class ObservationsListViewModel : ViewModel
    {
        public List<Observations> Observations { get; set; }
        public string Link { get; set; }
        public Organe Organe { get; set; }

        public ObservationsListViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/observation/select/";
        }
    }
}