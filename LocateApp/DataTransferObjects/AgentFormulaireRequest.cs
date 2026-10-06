using System.Diagnostics.CodeAnalysis;

namespace LocateApp.DataTransferObjects
{
    /// <summary>Formulaire d'ajout ou de modification d'un agent (/agent/add/, /agent/modify/).</summary>
    [ExcludeFromCodeCoverage]
    public class AgentFormulaireRequest
    {
        /// <summary>Matricule à 6 chiffres (sans le suffixe « 00 » de la table Agent) ; vide à l'ajout = attribué automatiquement.</summary>
        public string Matricule { get; set; }
        public string Nom { get; set; }
        public string Postnom { get; set; }
        public string Prenom { get; set; }
        /// <summary>1 = homme, 2 = femme.</summary>
        public int Sexe { get; set; }
        /// <summary>Date au format aaaa-mm-jj (champ date du navigateur), facultative.</summary>
        public string DateNaissance { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string CodeOrgane { get; set; }
    }
}
