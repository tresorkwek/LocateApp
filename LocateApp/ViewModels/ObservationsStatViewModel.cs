using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;
using LocateApp.DataTransferObjects;

namespace LocateApp.ViewModels
{
    public class ObservationsStatViewModel : ViewModel
    {
        public List<SelectStatObservationRequest> ObservationsStat { get; set; }
        public string Link { get; set; }

        public ObservationsStatViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/observation/select/";
        }
    }
}