using System.Collections.Generic;
using LocateApp.Controllers;
using LocateApp.Models;

namespace LocateApp.ViewModels
{
    /// <summary>Formulaire d'ajout (Agent nul) ou de modification d'un agent.</summary>
    public class AgentFrmViewModel : ViewModel
    {
        public Agent Agent { get; set; }
        public bool Ajout => Agent == null;
        /// <summary>Matricule proposé à l'ajout (le prochain libre).</summary>
        public string MatriculePropose { get; set; }
        /// <summary>Compte Locate de l'agent, s'il en a un (son identité suit celle de l'agent).</summary>
        public string Compte { get; set; }
        public List<Organe> Organigramme { get; set; }

        public AgentFrmViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Organigramme = OrganeController.Organigramme();
        }
    }
}
