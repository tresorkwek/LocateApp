using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Controllers;

namespace LocateApp.Models
{
    public class Inventaire
    {
        public int Annee { get; set; }
        public DateTime DateCreation { get; set; }
        public string UserCreation { get; set; }
        public DateTime DateCloture { get; set; }
        public string UserCloture { get; set; }
        public bool Actif { get; set; } 

        public List<InventaireDetails> GetDetails()
        {
            return InventaireController.SelectDetails(Annee);
        }
        public Identity GetIdentity(string UserName)
        {
            return UserName == null ? null : IdentityController.GetUser(UserName);
        }

        public bool ReadyToClose()
        {
            long quantiteImmo = ImmoController.SelectQuantite();
            long quantiteImmoInventorie = InventaireController.SelectQuantite();

            return quantiteImmo == quantiteImmoInventorie;
        }
    }
}