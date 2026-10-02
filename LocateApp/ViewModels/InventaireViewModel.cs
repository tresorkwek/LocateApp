using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class InventaireViewModel : ViewModel
    {
        public Inventaire Inventaire { get; set; }
        public string Link { get; set; }

        public InventaireViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/inventaire/modify/";
        }
    }
}