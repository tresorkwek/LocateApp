using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class ImmoListViewModel : ViewModel
    {
        public List<Immo> Immos { get; set; }
        public Local Local { get; set; }
        public Organe Organe { get; set; }
        /// <summary>Article sur lequel la liste est filtrée (vue d'un article dans un local ou un organe).</summary>
        public Article Article { get; set; }
        /// <summary>Liens du sélecteur « par article / en détails » (local ou organe).</summary>
        public string LienParArticle { get; set; }
        public string LienDetails { get; set; }
        public int AnneEnCours => InventaireController.SelectAnneeEnCours();
        public string Link { get; set; }
        public string ActionLink { get; set; }

        public ImmoListViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/immo/id/";
        }
    }
}