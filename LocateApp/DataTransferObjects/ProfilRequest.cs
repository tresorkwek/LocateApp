using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace LocateApp.DataTransferObjects
{
    [ExcludeFromCodeCoverage]
    public class ProfilSelectRequest
    {
        public int IdProfil { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class AddProfilRequest
    {
        public string Nom { get; set; }
        public string Libelle { get; set; }
        public bool Externe { get; set; }
        public string HomeUrl { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ModifyProfilRequest
    {
        public int IdProfil { get; set; }
        public string Nom { get; set; }
        public string Libelle { get; set; }
        public bool Externe { get; set; }
        public string HomeUrl { get; set; }
    }
}