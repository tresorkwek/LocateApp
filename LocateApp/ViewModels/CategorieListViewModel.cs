using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class CategorieListViewModel : ViewModel
    {
        public List<Categorie> Categories { get; set; }
        public string Link { get; set; }

        public CategorieListViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/categorie/id/";
        }
    }
}