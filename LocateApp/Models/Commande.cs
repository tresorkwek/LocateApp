using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Controllers;

namespace LocateApp.Models
{
    public class Commande
    {
        public Guid Id { get; set; }
        public long NumeroCommande { get; set; }
        public int NbrePage { get; set; }
        public int IdFormat { get; set; }
        public string CreatedBy { get; set; }
        public DateTime DateCreation { get; set; }
        public string ValidateBy { get; set; }
        public DateTime DateValidation { get; set; }
        public string PrintBy { get; set; }
        public DateTime DatePrint { get; set; }

        public EtiquetteFormat GetFormatEtiquette()
        {
            return EtiquetteFormatController.Select(IdFormat).FirstOrDefault();
        }

        public Identity GetIdentity(string UserName)
        {
            return UserName == null ? null : IdentityController.GetUser(UserName);
        }

        public bool IsValidate()
        {
            return NbrePage == GetNbrePageGenerees();
        }

        public long GetNbreEtiquette()
        {
            return CommandeController.SelectNbreEtiquette(Id);
        }

        public long GetNbreEtiquetteUsed()
        {
            return CommandeController.SelectNbreEtiquetteUsed(Id);
        }

        public long GetNbreEtiquetteNotUsed()
        {
            return CommandeController.SelectNbreEtiquetteNotUsed(Id);
        }

        public long GetNbrePageGenerees()
        {
            long nbreEtiquette = GetNbreEtiquette();
            EtiquetteFormat formatEtiquette = GetFormatEtiquette();

            return nbreEtiquette / (formatEtiquette.Ligne * formatEtiquette.Colone);
        }
    }
}