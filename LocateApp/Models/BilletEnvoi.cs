using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Controllers;

namespace LocateApp.Models
{
    public class BilletEnvoi
    {
        public long Id { get; set; }
        public string NumBilletEnvoi { get; set; }
        public string Matricule { get; set; }
        public string IdHopital { get; set; }
        public string CodeOrgane { get; set; }
        public string Organe { get; set; }
        public string UserCreation { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime DateExpiration { get; set; }
        public string UserAnnulation { get; set; }
        public DateTime DateAnnulation { get; set; }
        public bool IsCanceled { get; set; }
        public int PrintNumber { get; set; }
        public string Observation { get; set; } 
        public Guid IdBilletEnvoi { get; set; }
        public List<ServiceBilletEnvoi> ServiceBilletEnvois => GetServicesHopital();
        public Agent Patient { get; set; }
        public Organe Hopital { get; set; }
        public Agent Agent => GetAgent();
        public bool CanBeModified => GetModificationStatut();

        public BilletEnvoi()
        {
            Patient = GetPatient();
            //Hopital = GetHopital();
        }


        private List<ServiceBilletEnvoi> GetServicesHopital()
        {
            return ServiceBilletEnvoiController.GetServiceHopitalByBilletEnvoi(Id);
        }

        private Agent GetPatient()
        {
            List<Agent> patient = AgentController.SelectAgent(Matricule);

            return (patient.Count > 0)? patient.FirstOrDefault() : null;
        }
        private Agent GetAgent()
        {

            Identity userCreation = IdentityController.GetUser(UserCreation);

           // Identity medecinUser = string.IsNullOrEmpty(userCreation.NumOrdMed)? IdentityController.GetFocalPoint() : userCreation;

            Identity medecinUser = userCreation;

            Agent patient = AgentController.SelectAgent(medecinUser.UserName).FirstOrDefault();

            if (patient != null)
            {
                patient.Telephone = medecinUser.Telephone;
            }

            return patient;
        }
        //private Hopital GetHopital()
        //{
        //    List<Hopital> hopitals = OrganeController.Select(IdHopital);

        //    return (hopitals.Count > 0) ? hopitals[0] : null;
        //}

        private bool GetModificationStatut()
        {
            List<ServiceBilletEnvoi> listOfServiceBilletEnvois = GetServicesHopital();

            bool canBeModified = false;

            foreach (var serviceBilletEnvoi in listOfServiceBilletEnvois)
            {
                if (!serviceBilletEnvoi.IsUsed)
                {
                    canBeModified = true;
                    break;
                }
            }

            return canBeModified;
        }
    }
}