using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class ServiceMedicalViewModel : ViewModel
    {
        public List<ServiceMedical> ServiceMedical { get; set; }
        public string Link { get; set; }

        public ServiceMedicalViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/servicemedical/modify/";
        }
    }
}