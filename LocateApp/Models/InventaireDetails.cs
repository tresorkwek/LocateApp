using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Controllers;

namespace LocateApp.Models
{
    public class InventaireDetails
    {
        public Guid Id { get; set; }
        public int Annee { get; set; }
        public long IdImmo { get; set; }
        public bool ImmoExist { get; set; }
        public string Etat { get; set; }
        public int IdObservation { get; set; }
        public DateTime DateCreation { get; set; }
        public string UserCreation { get; set; }
        public string Responsable { get; set; }
        public long IdLocal { get; set; }
        public string CodeOrgane { get; set; }
        public string Observation { get; set; }
        public string DesignationLocal { get; set; }

        public Inventaire GetEntete()
        {
            return InventaireController.SelectEntete(Annee).FirstOrDefault();
        }
        public Immo GetImmo()
        {
            return ImmoController.SelectById(IdImmo);
        }

        public Observations GetObservations()
        {
            return ObservationsController.Select(IdObservation).FirstOrDefault();
        }

        public Local GetLocal()
        {
            return LocalController.SelectById(IdLocal).FirstOrDefault();
        }

        public Organe GetOrgane()
        {
            return OrganeController.SelectById(CodeOrgane);
        }
    }
}