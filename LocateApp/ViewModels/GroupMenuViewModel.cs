using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;

namespace LocateApp.ViewModels
{
    public class GroupMenuViewModel : ViewModel
    {
        public List<GroupMenu> GroupMenus { get; set; }
        public string Link { get; set; }

        public GroupMenuViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {            
            Link = "menu/groupmenu/";
        }
    }
}