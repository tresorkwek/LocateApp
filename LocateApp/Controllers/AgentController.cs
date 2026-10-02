using System;
using System.Collections.Generic;
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

                valuesOfSelectPatient = new MatriculePatientRequest
                {
                    Matricule = matricule + "%"
                };

                sql = SqlAgent.SelectById;
            }
            else
            {
                valuesOfSelectPatient = new NamePatientRequest
                {
                    Nom = valueToSelect + "%"
                };

                sql = SqlAgent.SelectByName;
            }                      

            return SqlDataAccess.SelectData<Agent>(sql,valuesOfSelectPatient,null,1);
        }


        public static List<Agent> SelectPatientBySerialId(string id)
        {

            List<Agent> resultQuery = SqlDataAccess.SelectData<Agent>(SqlPatient.SelectPatientBySerialId, new { Id = id }, null, 2);

            return resultQuery;
        }

        public static List<Agent> SelectAgentByOrgane(string CodeOrgane = null)
        {
            string sql = CodeOrgane == null ? SqlAgent.SelectAll : SqlAgent.SelectByOrgane;

            return SqlDataAccess.SelectData<Agent>(sql, new { CodeOrgane }, null, 1);
        }

        public static Direction SelectOrganeOfAgent(string matricule)
        {
            if (!Utilities.ForString.IsNip(matricule)) // vérifier si c'est un nip            
            {
                //Todo Gérer l'exeption 
            }

            object nipOfSelectPatient = new MatriculePatientRequest
            {
                Matricule = matricule.Substring(0,6)+"00"
            };

            List<Direction> resultQuery = SqlDataAccess.SelectData<Direction>(SqlDirection.SelectByAgent, nipOfSelectPatient, null, 2);

            return resultQuery.FirstOrDefault();
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

            List<Agent> agentMovia = SqlDataAccess.SelectData<Agent>(sql, valuesOfSelectPatient, null, 2);
            List<Identity> identities = IdentityController.GetIdentity();

            List<Agent> agentNotUser = new List<Agent>();

            agentNotUser = agentMovia;

            foreach (var identity in identities)
            {
                var agentToRemove = agentMovia.Find(t => t.Matricule == identity.UserName + "00");
                if (agentToRemove != null) agentNotUser.Remove(agentToRemove);
            }  
            
            return agentNotUser;
        }

        public static List<Agent> SelectResponsableByOrgane(string CodeOrgane)
        {            
           
            List<string> matriculeResponsable = ImmoController.SelectResponsableByOrgane(CodeOrgane);

            List<Agent> responsable = new List<Agent>();

            matriculeResponsable = matriculeResponsable.Where(m => !string.IsNullOrWhiteSpace(m)).Select(m => m.Trim()).Distinct().ToList();

            if(matriculeResponsable.Count > 0)
            {
                List<Agent> agentMovia = SelectAgentByOrgane(CodeOrgane);

                // Un responsable peut être affecté (RH) ailleurs que dans l'organe où sont ses biens, et les codes d'organe
                // de Movia peuvent ne pas suivre l'organigramme de Locate : les matricules non trouvés sont cherchés parmi tous les agents.
                if (matriculeResponsable.Any(m => !agentMovia.Exists(t => t.Matricule?.Trim() == m + "00")))
                {
                    agentMovia = agentMovia.Concat(SelectAgentByOrgane()).ToList();
                }

                foreach (var matricule in matriculeResponsable)
                {
                    var agentToAdd = agentMovia.Find(t => t.Matricule?.Trim() == matricule + "00");
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
                List<Agent> agentMovia = SelectAgentByOrgane(CodeOrgane);
                int i = 0;
                
                foreach (var agentResponsable in matriculeResponsable)
                {
                    i++;
                    var agentToAdd = agentMovia.Find(t => t.Matricule.Contains(agentResponsable.Matricule));
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