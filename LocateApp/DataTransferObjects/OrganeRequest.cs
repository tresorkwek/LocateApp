using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace LocateApp.DataTransferObjects
{

    [ExcludeFromCodeCoverage]
    public class UpSetOrganeRequest
    {
        public string Id { get; set; }
        public string Nom { get; set; }
        public string Sigle { get; set; }
        public string IndCoresp { get; set; }
        public bool Fictif { get; set; }
        public bool Actif { get; set; }
        public int IdTypeOrgane { get; set; }
        public string IdOrganeParent { get; set; }
        public bool Interne { get; set; }
        public int Ordre { get; set; }
        public string IdStructure { get; set; }
        public int OrdreInterne { get; set; }
        public string Adresse { get; set; }
        public int Version { get; set; }
        public bool Entite { get; set; }
    
    }

    /// <summary>Un organe déplacé dans l'organigramme : sa nouvelle position (parent + ordre parmi ses frères).</summary>
    [ExcludeFromCodeCoverage]
    public class OrganeReorderRequest
    {
        public string Id { get; set; }
        public string Parent { get; set; }
        public int Ordre { get; set; }
    }

    /// <summary>Liste à plat envoyée par la vue de l'organigramme après un glisser-déposer.</summary>
    [ExcludeFromCodeCoverage]
    public class OrganigrammeReorderRequest
    {
        public List<OrganeReorderRequest> Items { get; set; }
    }
}
