using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class FamilleModifyFrmViewModel : ViewModel
    {
        public Famille Famille { get; set; }
        public string Link { get; set; }

        public FamilleModifyFrmViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/famille/add/";
        }
    }
}