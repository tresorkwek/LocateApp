using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Controllers;

namespace LocateApp.Models
{
    public abstract class AyantDroit
    {
        public Guid SerialId { get; set; }
        public string Matricule { get; set; }
        public string Nom { get; set; }
        public string Postnom { get; set; }
        public string Prenom { get; set; }
        public string Sexe { get; set; }
        public string DateNaissance { set; get; }
        public int SituationFamiliale { get; set; }
        public string Adresse { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        
    }

}