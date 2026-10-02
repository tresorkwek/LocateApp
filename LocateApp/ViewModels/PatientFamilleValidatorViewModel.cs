using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;
using LocateApp.Utilities;

namespace LocateApp.ViewModels
{
    public class PatientFamilleValidatorViewModel:ViewModel
    {
        public List<Agent> Patients { get; set; }
        public List<Organe> Hopitaux => OrganeController.Select();
        public List<Agent> NonPrisEnCharge => GetNonPrisEnCharge();
        public List<DataClient> EtatCivil => GetEtatCivil();
        public List<DataClient> Sexe => GetSexe();
        public bool IsModification { get; set; }
        public string Button => GetButtonName();
        public string InputStyle => GetInputStyle();
        public string SelectStyle => GetSelectStyle();
        public string CheckBoxStyle => GetCheckBoxStyle();


        public PatientFamilleValidatorViewModel(List<Agent> listOfPatient, string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Patients = listOfPatient;
            IsModification = false;            
        }

        private List<Agent> GetNonPrisEnCharge()
        {
            List<Agent> nonPriseEnCharge = new List<Agent>();

           // foreach (var patient in Patients)
           // {
           //     if (patient.PrisEnCharge == "NON") nonPriseEnCharge.Add(patient);
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
        
        private string GetInputStyle()
        {
            return IsModification ? "padding-top:10px;padding-bottom:0px;" : "";
        }            

        private string GetSelectStyle()
        {
            return IsModification ? "padding-top:20px;padding-bottom:0px;" : "";
        }

        private string GetCheckBoxStyle()
        {
            return IsModification ? "padding-top:25px;padding-bottom:16px;" : "";
        }
        private string GetButtonName()
        {
            return IsModification ? "Modifier" : "Valider";
        }
    }
}