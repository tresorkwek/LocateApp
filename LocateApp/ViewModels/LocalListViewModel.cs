using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class LocalListViewModel : ViewModel
    {
        public List<Local> Locaux { get; set; }
        public string Link { get; set; }
        public Organe Organe { get; set; }
        public int AnneEnCours => InventaireController.SelectAnneeEnCours();

        public LocalListViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/immo/local/";
        }
    }
}