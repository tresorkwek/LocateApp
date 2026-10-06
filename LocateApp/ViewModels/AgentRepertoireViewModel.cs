using System.Collections.Generic;
using LocateApp.Models;

namespace LocateApp.ViewModels
{
    /// <summary>Répertoire des agents (Paramètres > Agents).</summary>
    public class AgentRepertoireViewModel : ViewModel
    {
        public List<AgentRepertoire> Agents { get; set; } = new List<AgentRepertoire>();
        public string LienBiens { get; set; } = "/immo/responsable/bien/";

        public AgentRepertoireViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
        }
    }
}
