using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Controllers;
using LocateApp.Models;

namespace LocateApp.ViewModels
{
    public class ArticleListViewModel : ViewModel
    {
        public List<Article> Articles { get; set; }
        public string Link { get; set; }
        public int AnneEnCours => InventaireController.SelectAnneeEnCours();
        public Local Local { get; set; }
        public Organe Organe { get; set; }
        /// <summary>Liens du sélecteur « par article / en détails » (local ou organe).</summary>
        public string LienParArticle { get; set; }
        public string LienDetails { get; set; }
        /// <summary>Biens inventoriés par article quand la liste ne porte pas sur un local (organe).</summary>
        public Dictionary<long, long> InventoriesParArticle { get; set; }

        public ArticleListViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/article/id/";
        }

    }
}