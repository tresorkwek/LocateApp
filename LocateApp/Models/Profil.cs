using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Models
{
    public class Profil
    {
        public int IdProfil { get; set; }
        public string Nom { get; set; }
        public string Libelle { get; set; }
        public bool Root { get; set; }
        public bool DefaultProfil { get; set; }
        public bool Externe { get; set; }
        public string HomeUrl { get; set; }
    }
}