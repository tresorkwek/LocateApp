using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;
using System.Configuration;

namespace LocateApp.ViewModels
{
    public class ImmoAddFrmViewModel : ViewModel
    {
        public List<Immo> ImmoAutonome { get; set; }
        public string Link { get; set; }
        public bool IsModification { get; set; }
        public List<Article> Articles => ArticleController.Select();
        public Local Local { get; set; }
        public List<Agent> Occupants { get; set; }
        public List<Observations> Observations => ObservationsController.SelectByEtat("B");
        public int InventaireEnCours => InventaireController.SelectAnneeEnCours();
        public string InputStyle => GetInputStyle();
        public string SelectStyle => GetSelectStyle();
        public string CheckBoxStyle => GetCheckBoxStyle();

        public ImmoAddFrmViewModel(string userName, MessageAlerte messageAlerte = null): base(userName, messageAlerte)
        {
            Link = "/immo/add/";
            
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