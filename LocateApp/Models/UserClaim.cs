using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Controllers;

namespace LocateApp.Models
{
    public class UserClaim
    {
        public int IdClaim { get; set; }
        public int IdProfil { get; set; }
        public int IdMenu { get; set; }
        public string CustomUrl { get; set; }
        public Profil Profil => GetProfil();
        public Menu  Menu => GetMenu();


        private Profil GetProfil()
        {
            return ProfilController.GetProfil(IdProfil).FirstOrDefault();
        }

        private Menu GetMenu()
        {
            return MenuController.GetMenu(IdMenu).FirstOrDefault();
        }

    }
}