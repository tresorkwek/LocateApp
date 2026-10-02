using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Models
{
    public class Categorie
    {
        public long Id { get; set; }
        public string Designation { get; set; }
        public bool IsActive { get; set; }
        public long IdFamille { get; set; }
        public string Famille { get; set; }
        public DateTime DateCreation { get; set; }
        public string UserCreation { get; set; }
    }
}