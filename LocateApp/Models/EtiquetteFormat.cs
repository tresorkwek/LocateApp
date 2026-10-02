using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Models
{
    public class EtiquetteFormat
    {
        public int Id{ get; set; }
        public string Nom { get; set; }
        public int Ligne { get; set; }
        public int Colone { get; set; }
        public int Largeur { get; set; }
        public int Hauteur { get; set; }
        public string Papier { get; set; }
        public bool IsPublic { get; set; }
    }
}