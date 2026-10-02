using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class LocalViewModel : ViewModel
    {
        public Local Local { get; set; }
        public string Link { get; set; }
        public int AnneEnCours => InventaireController.SelectAnneeEnCours();

        public LocalViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/immo/local/";
        }
    }
}