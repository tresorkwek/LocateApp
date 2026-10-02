using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.ViewModels
{
    public class AgentListViewModel:ViewModel
    {
        public List<Agent> Agents { get; set; }
        public string Link { get; set; }
        public int AnneEnCours = InventaireController.SelectAnneeEnCours();

        public AgentListViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/patient/select/";
        }
    }
}