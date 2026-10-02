using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class MenuAddFrmViewModel : ViewModel
    {
        public List<GroupMenu> GroupMenus => GroupMenuController.GetAllGroupMenu();
        public List<Module> Modules => ModuleController.GetModules();
        public List<Actions> Actions => ActionController.GetAction();

        public string Link { get; set; }

        public MenuAddFrmViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/menu/modify/";
            
        }


    }
}