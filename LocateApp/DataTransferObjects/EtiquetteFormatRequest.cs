using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace LocateApp.DataTransferObjects
{   

    [ExcludeFromCodeCoverage]
    public class EtiquetteFormatInsertRequest
    {
        public string Nom { get; set; }
        public int Ligne { get; set; }
        public int Colone { get; set; }
        public int Largeur { get; set; }
        public int Hauteur { get; set; }
        public string Papier { get; set; }
        public bool IsPublic { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class EtiquetteFormatModifytRequest
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public int Ligne { get; set; }
        public int Colone { get; set; }
        public int Largeur { get; set; }
        public int Hauteur { get; set; }
        public string Papier { get; set; }
        public bool IsPublic { get; set; }
    }
}