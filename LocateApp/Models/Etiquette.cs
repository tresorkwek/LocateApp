using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Controllers;

namespace LocateApp.Models
{
    public class Etiquette
    {
        public long Id { get; set; }
        public Guid QrCode { get; set; }
        public Guid IdCommande { get; set; }
        public int IdFormat { get; set; }
        public DateTime DateCreation { get; set; }
        public string UserCreation { get; set; }
        public bool Printed { get; set; }
        public DateTime DatePrint { get; set; }
        public string UserPrint { get; set; }
        public bool IsUsed { get; set; }
        public string UsedBy { get; set; }
        public DateTime DateUsed { get; set; }
        public string TypeUsed { get; set; }

        public EtiquetteFormat GetFormatEtiquette()
        {
            return EtiquetteFormatController.Select(IdFormat).FirstOrDefault();
        }

        public Identity GetIdentity(string UserName)
        {
            return UserName == null ? null : IdentityController.GetUser(UserName);
        }

        public Commande GetCommande()
        {
            return CommandeController.Select(IdCommande).FirstOrDefault();
        }
    }
}