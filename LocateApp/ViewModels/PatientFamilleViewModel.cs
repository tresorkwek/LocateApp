using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;
using LocateApp.Utilities;

namespace LocateApp.ViewModels
{
    public class PatientFamilleViewModel : ViewModel
    {
        public List<Agent> Patients { get; set; }
        public List<Organe> Hopitaux => OrganeController.Select();
        public List<Agent> NonPrisEnCharge => GetNonPrisEnCharge();
        public List<DataClient> EtatCivil => GetEtatCivil();
        public List<DataClient> Sexe => GetSexe();
        public string Link { get; set; }
        public string ModifyLink { get; set; }
        public string AccordLink { get; set; }
        public bool IsMyProfil { get; set; }
        public bool CanModify => GetModifyStatus();
        public bool CanEtablishBE => GetEtablishBEStatus();
        public bool CanValidate => GetValidateStatus();


        public PatientFamilleViewModel(List<Agent> listOfPatient,  string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Patients = listOfPatient;
            Link = "/billetenvoi/create/";
        }

        private List<Agent> GetNonPrisEnCharge()
        {
            List<Agent> nonPriseEnCharge = new List<Agent>();
            
            //foreach (var patient in Patients)
            //{
            //    if (patient.PrisEnCharge == "NON") nonPriseEnCharge.Add(patient);
            //}

            return nonPriseEnCharge;
        }

        private List<DataClient> GetEtatCivil()
        {
            List<DataClient> listOfEtatCivil = new List<DataClient>
            {
                new DataClient() { Value = "1", Label = "Célibataire" },
                new DataClient() { Value = "2", Label = "Marié(e)" },
                new DataClient() { Value = "3", Label = "Veuf(ve)" },
                new DataClient() { Value = "4", Label = "Divorcé(e)" }
            };

            return listOfEtatCivil;
        }

        private List<DataClient> GetSexe()
        {
            List<DataClient> listOfSexe = new List<DataClient>
            {
                new DataClient() { Value = "1", Label = "Masculin" },
                new DataClient() { Value = "2", Label = "Féminin" }
            };

            return listOfSexe;
        }

        private bool GetModifyStatus()
        {
            return !IsMyProfil && Utilities.Utilities.CheckClaimStatus(Identity, "GetPatientModifyMatricule");
        }

        private bool GetEtablishBEStatus()
        {
            return !IsMyProfil && Utilities.Utilities.CheckClaimStatus(Identity, "GetBilletenvoiCreateNip");
        }
        private bool GetValidateStatus()
        {
            return !IsMyProfil && Utilities.Utilities.CheckClaimStatus(Identity, "GetPatientAccordNip");
        }
    }
}