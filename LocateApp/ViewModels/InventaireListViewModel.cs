using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class InventaireListViewModel : ViewModel 
    {
        public List<Inventaire> Inventaires { get; set; }
        public List<InventaireDetails> InventaireDetails { get; set; }
        public int AnneeEnCours => InventaireController.SelectAnneeEnCours();
        public string Link { get; set; }
        public string ActionLink { get; set; }

        public InventaireListViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/inventaire/details/";
        }
    }
}