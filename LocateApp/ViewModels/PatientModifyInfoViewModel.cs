using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;

namespace LocateApp.ViewModels
{
    public class PatientModifyInfoViewModel:ViewModel
    {
        public Agent Patient { get; set; }
        public string Link { get; set; }

        public PatientModifyInfoViewModel(string userName, MessageAlerte messageAlerte = null): base(userName, messageAlerte)
        {

        }
    }
}