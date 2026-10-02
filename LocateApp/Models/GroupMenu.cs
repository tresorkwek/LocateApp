using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Controllers;

namespace LocateApp.Models
{
    public class GroupMenu
    {
        public int IdGroupMenu { get; set; }
        public string Nom { get; set; }
        public string Libelle { get; set; }
        public string Url { get; set; }
        public string Icone { get; set; }
        public bool Visible { get; set; }
        public int IdSection { get; set; }
        public bool DefaultInternalUser { get; set; }
        public bool DefaultExternalUser { get; set; }
        public List<Menu> Menu { get; set; }
        public string Section => GetSection();
        public List<Menu> AllMenu => GetAllMenuByGroupMenu();

        public GroupMenu()
        {
            Menu = GetMenuByGroupMenu(IdGroupMenu);
        }

        private string GetSection()
        {
            return SectionController.GetSection(IdSection).FirstOrDefault().Nom;
        }
        private List<Menu> GetMenuByGroupMenu(int idGroupMenu)
        {
            return MenuController.GetMenuByGroupMenu(idGroupMenu);
        }
        private List<Menu> GetAllMenuByGroupMenu()
        {
            return MenuController.GetAllMenuByGroupMenu(IdGroupMenu);
        }

    }
}