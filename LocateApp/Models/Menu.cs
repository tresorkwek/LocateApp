using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Controllers;

namespace LocateApp.Models
{
    public class Menu
    {
        public int IdMenu { get; set; }
        public string Nom { get; set; }
        public string Libelle { get; set; }
        public string Icone { get; set; }
        public string Commentaire { get; set; }
        public string Titre { get; set; }
        public string Url { get; set; }
        public int IdGroupMenu { get; set; }
        public int Ordre { get; set; }
        public bool Visible { get; set; }
        public int IdModule { get; set; }
        public int IdAction { get; set; }
        public bool DefaultMenu { get; set; }
        public string Module => GetModule();
        public string  Action => GetAction();
        public string  GroupMenu => GetGroupMenu();

        private string GetModule()
        {
            return ModuleController.GetModules(IdModule).FirstOrDefault().Nom;
        }

        private string GetAction()
        {
            return ActionController.GetAction(IdAction).FirstOrDefault().Nom;
        }

        private string GetGroupMenu()
        {
            return GroupMenuController.GetAllGroupMenu(IdGroupMenu).FirstOrDefault().Nom;
        }
    }
}