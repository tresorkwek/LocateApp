using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class ServiceMedicalModifyFrmViewModel : ViewModel
    {
        public ServiceMedical ServiceMedical { get; set; }
        public string Link { get; set; }

        public ServiceMedicalModifyFrmViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            
        }
    }
}