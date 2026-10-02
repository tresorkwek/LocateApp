using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class ArticleViewModel : ViewModel
    {
        public Article Article { get; set; }
        public string Link { get; set; }
        public List<AchatArticle> Achats { get; set; } = new List<AchatArticle>();
        /// <summary>Dernier prix d'achat ramené en devise de référence.</summary>
        public decimal? DernierPrix => Achats.Count > 0 ? Achats[0].PrixReference : (decimal?)null;
        public string Devise => AchatController.Devise;

        public ArticleViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/article/modify/";
        }
    }
}