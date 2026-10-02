using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Models
{
    public class Fournisseur
    {
        public long Id { get; set; }
        public string Nom { get; set; }
        public string Adresse { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateCreation { get; set; }
        public string UserCreation { get; set; }
    }
}