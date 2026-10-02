using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class CategorieAddOrModifyFrmViewModel : ViewModel
    {
        public Categorie Categorie { get; set; }
        public List<Famille> Familles => FamilleController.Select();
        public string Link { get; set; }

        public CategorieAddOrModifyFrmViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/categorie/add/";
        }
    }
}