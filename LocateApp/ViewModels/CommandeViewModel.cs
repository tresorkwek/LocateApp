using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;

namespace LocateApp.ViewModels
{
    public class CommandeViewModel : ViewModel
    {
        public List<Commande> Commandes { get; set; }
        public string Link { get; set; }

        public CommandeViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/commande/modify/";
        }
    }
}