using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class EtiquetteViewModel : ViewModel
    {
        public List<Etiquette> Etiquettes { get; set; }
        public Commande Commande { get; set; }
        public string Link { get; set; }

        public EtiquetteViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/etiquette/print/";
        }
    }
}