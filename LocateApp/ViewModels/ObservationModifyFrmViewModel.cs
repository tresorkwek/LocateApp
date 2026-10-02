using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;

namespace LocateApp.ViewModels
{
    public class ObservationModifyFrmViewModel : ViewModel
    {
        public Observations Observations { get; set; }
        public string Link { get; set; }

        public ObservationModifyFrmViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/observation/modify/";
        }
    }
}