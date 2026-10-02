using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Models
{
    public class Famille
    {
        public long Id { get; set; }
        public string Nom { get; set; }
        public bool IsActive { get; set; }
        public string Couleur { get; set; }
        public long Nbre { get; set; }
    }
}