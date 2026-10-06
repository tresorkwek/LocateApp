using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.DataTransferObjects;
using System.Text.RegularExpressions;
using LocateApp.Repository;
using LocateApp.Utilities;

namespace LocateApp.Controllers
{
    public static class AgentController
    {

        public static List<Agent> SelectAgent(string valueToSelect = null)
        {
            object valuesOfSelectPatient=null;
            string sql;

            if (valueToSelect == null)
            {
                sql = SqlAgent.SelectAll;

            }
            else if(Utilities.ForString.IsId(valueToSelect)) // vérifier si c'est un nombre pour le matricule
            {
                string matricule = (valueToSelect.Length == 7) ? valueToSelect.Substring(0, 6) : valueToSelect;

                valuesOfSelectPatient = new { Matricule = matricule + "%" };

                sql = SqlAgent.SelectById;
            }
            else
            {
                valuesOfSelectPatient = new { Nom = valueToSelect + "%" };

                sql = SqlAgent.SelectByName;
            }                      

            return SqlDataAccess.SelectData<Agent>(sql,valuesOfSelectPatient,null,1);
        }


        public static List<AgentRepertoire> SelectRepertoire()
        {
            return SqlDataAccess.SelectData<AgentRepertoire>(SqlAgent.SelectRepertoire, null, null, 1);
        }

        public static Agent SelectByMatricule(string matricule)
        {
            return SqlDataAccess.SelectData<Agent>(SqlAgent.SelectByMatricule, new { Matricule = matricule?.Trim() }, null, 1).FirstOrDefault();
        }

        /// <summary>Prochain matricule libre (ni agent, ni compte utilisateur), à la suite du plus grand matricule numérique.</summary>
        public static string ProchainMatricule()
        {
            int numero = SqlDataAccess.SelectData<int>(SqlAgent.DernierMatricule, null, null, 1).FirstOrDefault();
            string candidat;
            do
            {
                numero++;
                candidat = numero.ToString("000000");
            }
            while (MatriculeUtilise(candidat) && numero < 999999);
            return candidat;
        }

        public static bool MatriculeUtilise(string matricule)
        {
            return SqlDataAccess.SelectData<int>(SqlAgent.MatriculeUtilise, new { Matricule = matricule }, null, 1).FirstOrDefault() > 0;
        }

        /// <summary>Contrôle du formulaire ; renvoie le message d'erreur, ou null si tout est correct. Normalise les valeurs saisies.</summary>
        public static string Valider(AgentFormulaireRequest agent, bool ajout)
        {
            agent.Matricule = agent.Matricule?.Trim();
            agent.Nom = agent.Nom?.Trim().ToUpper();
            agent.Postnom = agent.Postnom?.Trim().ToUpper();
            agent.Prenom = string.IsNullOrWhiteSpace(agent.Prenom) ? null : agent.Prenom.Trim();
            agent.Telephone = string.IsNullOrWhiteSpace(agent.Telephone) || agent.Telephone.Contains("_") ? null : agent.Telephone.Trim();
            agent.Email = string.IsNullOrWhiteSpace(agent.Email) ? null : agent.Email.Trim().ToLower();
            agent.CodeOrgane = string.IsNullOrWhiteSpace(agent.CodeOrgane) ? null : agent.CodeOrgane.Trim();

            if (ajout && string.IsNullOrEmpty(agent.Matricule)) { agent.Matricule = ProchainMatricule(); }

            if (string.IsNullOrEmpty(agent.Matricule) || !Regex.IsMatch(agent.Matricule, @"^\d{6}$")) { return "Le matricule doit comporter exactement 6 chiffres."; }
            if (ajout && MatriculeUtilise(agent.Matricule)) { return $"Le matricule {agent.Matricule} est déjà attribué à un agent ou à un compte."; }
            if (!ajout && SelectByMatricule(agent.Matricule) == null) { return $"Aucun agent n'a le matricule {agent.Matricule}."; }
            if (string.IsNullOrEmpty(agent.Nom)) { return "Le nom est obligatoire."; }
            if (string.IsNullOrEmpty(agent.Postnom)) { return "Le postnom est obligatoire."; }
            if (agent.Nom.Length > 50 || agent.Postnom.Length > 50 || (agent.Prenom?.Length ?? 0) > 50) { return "Le nom, le postnom et le prénom sont limités à 50 caractères."; }
            if (agent.Sexe != 1 && agent.Sexe != 2) { return "Indiquez le sexe de l'agent."; }
            if (string.IsNullOrEmpty(agent.CodeOrgane) || OrganeController.SelectById(agent.CodeOrgane) == null) { return "Choisissez l'organe d'affectation de l'agent."; }
            if ((agent.Telephone?.Length ?? 0) > 20) { return "Le numéro de téléphone est trop long (20 caractères au plus)."; }
            if (agent.Email != null && (agent.Email.Length > 100 || !Regex.IsMatch(agent.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))) { return "L'adresse e-mail n'est pas valide."; }

            DateTime? naissance = DateNaissance(agent);
            if (!string.IsNullOrWhiteSpace(agent.DateNaissance) && naissance == null) { return "La date de naissance n'est pas valide."; }
            if (naissance != null && (naissance.Value.Year < 1900 || naissance.Value > DateTime.Today.AddYears(-14))) { return "La date de naissance n'est pas plausible."; }

            return null;
        }

        private static DateTime? DateNaissance(AgentFormulaireRequest agent)
        {
            if (string.IsNullOrWhiteSpace(agent.DateNaissance)) { return null; }
            DateTime d;
            string[] formats = { "yyyy-MM-dd", "dd/MM/yyyy" };
            return DateTime.TryParseExact(agent.DateNaissance.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out d) ? d : (DateTime?)null;
        }

        private static object Valeurs(AgentFormulaireRequest agent)
        {
            return new
            {
                SerialId = Guid.NewGuid(),
                agent.Matricule,
                agent.Nom,
                agent.Postnom,
                agent.Prenom,
                agent.Sexe,
                SexeLettre = agent.Sexe == 1 ? "M" : "F",
                DateNaissance = DateNaissance(agent),
                agent.Telephone,
                agent.Email,
                agent.CodeOrgane
            };
        }

        public static bool Inserer(AgentFormulaireRequest agent, Identity identity)
        {
            return SqlDataAccess.SaveData(SqlAgent.Insert, Valeurs(agent), identity) > 0;
        }

        /// <summary>Modifie l'agent et, s'il a un compte Locate, reporte son identité et ses coordonnées sur le compte.</summary>
        public static bool Modifier(AgentFormulaireRequest agent, Identity identity)
        {
            object valeurs = Valeurs(agent);
            var requetes = new List<(string, object)> { (SqlAgent.Update, valeurs), (SqlAgent.UpdateCompte, valeurs) };
            return SqlDataAccess.SaveDataWithTransaction(requetes, identity) > 0;
        }

        public static List<Agent> SelectAgentBySerialId(string id)
        {
            return SqlDataAccess.SelectData<Agent>(SqlAgent.SelectBySerialId, new { Id = id }, null, 1);
        }

        public static List<Agent> SelectAgentByOrgane(string CodeOrgane = null)
        {
            string sql = CodeOrgane == null ? SqlAgent.SelectAll : SqlAgent.SelectByOrgane;

            return SqlDataAccess.SelectData<Agent>(sql, new { CodeOrgane }, null, 1);
        }

        public static List<Agent> SelectAgentActifNotUser(string valueToSelect = null)
        {
            object valuesOfSelectPatient = null;
            string sql = SqlAgent.SelectAllActifNotUser;

            if (valueToSelect != null)
            {
                sql = SqlAgent.SelectAllActifNotUserById;
                valuesOfSelectPatient = new { Matricule = valueToSelect + "%" };
            }

            // Agents de la table Agent qui n'ont pas encore de compte (la requête écarte déjà les utilisateurs existants)
            return SqlDataAccess.SelectData<Agent>(sql, valuesOfSelectPatient, null, 1);
        }

        public static List<Agent> SelectResponsableByOrgane(string CodeOrgane)
        {            
           
            List<string> matriculeResponsable = ImmoController.SelectResponsableByOrgane(CodeOrgane);

            List<Agent> responsable = new List<Agent>();

            matriculeResponsable = matriculeResponsable.Where(m => !string.IsNullOrWhiteSpace(m)).Select(m => m.Trim()).Distinct().ToList();

            if(matriculeResponsable.Count > 0)
            {
                List<Agent> agentsOrgane = SelectAgentByOrgane(CodeOrgane);

                // Un responsable peut être affecté (RH) ailleurs que dans l'organe où sont ses biens, et les codes d'organe
                // saisis à la main peuvent ne pas suivre l'organigramme de Locate : les matricules non trouvés sont cherchés parmi tous les agents.
                if (matriculeResponsable.Any(m => !agentsOrgane.Exists(t => t.Matricule?.Trim() == m + "00")))
                {
                    agentsOrgane = agentsOrgane.Concat(SelectAgentByOrgane()).ToList();
                }

                foreach (var matricule in matriculeResponsable)
                {
                    var agentToAdd = agentsOrgane.Find(t => t.Matricule?.Trim() == matricule + "00");
                    if (agentToAdd != null) responsable.Add(agentToAdd);
                }
            }

            return responsable;
        }

        public static List<Agent> SelectResponsableLitigieux(int? nbre = null, string CodeOrgane = null)
        {

            List<ResponsabletRequest> matriculeResponsable = ImmoController.SelectResponsableLitigieux(CodeOrgane);

            List<Agent> responsable = new List<Agent>();

            if (matriculeResponsable.Count > 0)
            {
                List<Agent> agentsOrgane = SelectAgentByOrgane(CodeOrgane);
                int i = 0;
                
                foreach (var agentResponsable in matriculeResponsable)
                {
                    i++;
                    var agentToAdd = agentsOrgane.Find(t => t.Matricule.Contains(agentResponsable.Matricule));
                    if (agentToAdd != null) responsable.Add(agentToAdd);

                    if (nbre != null)
                    {
                        if (nbre == i) break;
                    }
                }
            }

            return responsable;
        }

    }
}