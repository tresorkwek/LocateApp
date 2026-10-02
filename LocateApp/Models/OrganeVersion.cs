using System;

namespace LocateApp.Models
{
    /// <summary>Une version complète de l'organigramme ; une seule est courante (Defaut = 1).</summary>
    public class OrganeVersion
    {
        public int Id { get; set; }
        public bool Defaut { get; set; }
        public string Libelle { get; set; }
        public string Commentaire { get; set; }
        public DateTime? DateCreation { get; set; }
        public string UserCreation { get; set; }
        public int NbOrgane { get; set; }

        public string LibelleAffiche => string.IsNullOrWhiteSpace(Libelle) ? "Version " + Id : Libelle;
    }
}
