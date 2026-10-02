using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;
using System.Configuration;

namespace LocateApp.ViewModels
{
    public class ArticleModifyFrmViewModel : ViewModel
    {
        public Article Article { get; set; }
        public string Link { get; set; }
        public bool IsModification { get; set; }
        public List<Categorie> Categories { get; set; }
        public List<Fournisseur> Fournisseurs { get; set; }
        public string InputStyle => GetInputStyle();
        public string SelectStyle => GetSelectStyle();
        public string CheckBoxStyle => GetCheckBoxStyle();

        public ArticleModifyFrmViewModel(string userName, MessageAlerte messageAlerte = null): base(userName, messageAlerte)
        {
            Link = "/article/modify/";
            Categories = CategorieController.Select();
            Fournisseurs = FournisseurController.Select();
        }

        private string GetInputStyle()
        {
            return IsModification ? "padding-top:10px;padding-bottom:0px;" : "";
        }

        private string GetSelectStyle()
        {
            return IsModification ? "padding-top:20px;padding-bottom:0px;" : "";
        }

        private string GetCheckBoxStyle()
        {
            return IsModification ? "padding-top:25px;padding-bottom:16px;" : "";
        }

    }
}