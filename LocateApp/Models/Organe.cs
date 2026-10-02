using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Controllers;

namespace LocateApp.Models
{
    public class Organe : Institution
    {
        public string IndCoresp { get; set; }
        public bool Fictif { get; set; }
        public bool Actif { get; set; }
        public int IdTypeOrgane { get; set; }
        public string IdOrganeParent { get; set; }
        public bool Interne { get; set; }
        public int Ordre { get; set; }
        public string IdStructure { get; set; }
        public int OrdreInterne { get; set; }
        public long NbreBienInventorie { get; set; }
        public long NbreBienIdentifie { get; set; }
        public long NbreBienExistant { get; set; }
        public long NbreBienNonVu { get; set; }
        public long NbreBien { get; set; }
        public int Version { get; set; }
        public bool Entite { get; set; }
        public string NomStructure => OrganeController.SelectNameById(IdStructure);
        public string NomTypeOrgane => OrganeController.SelectType(Id);
        public int NbreLocaux => OrganeController.SelectNbreLocaux(Id).FirstOrDefault();
        public List<Organe> Organes { get; set; }

        public long GetQuantiteImmoInventorie()
        {
            return InventaireController.SelectQuantiteByOrgance(Id);
        }

    
    }

    /// <summary>Éléments rattachés à un organe, qui interdisent sa suppression physique.</summary>
    public class OrganeDependances
    {
        public int Enfants { get; set; }
        public int Rattaches { get; set; }
        public int Locaux { get; set; }
        public int Utilisateurs { get; set; }
        public int Inventaires { get; set; }

        public bool EstSupprimable => Enfants == 0 && Rattaches == 0 && Locaux == 0 && Utilisateurs == 0 && Inventaires == 0;

        public string Detail
        {
            get
            {
                var parts = new List<string>();
                if (Enfants > 0) parts.Add($"{Enfants} sous-organe(s)");
                if (Rattaches > 0) parts.Add($"{Rattaches} organe(s) rattaché(s) à cette structure");
                if (Locaux > 0) parts.Add($"{Locaux} local(aux)");
                if (Utilisateurs > 0) parts.Add($"{Utilisateurs} utilisateur(s)");
                if (Inventaires > 0) parts.Add($"{Inventaires} ligne(s) d'inventaire");
                return string.Join(", ", parts);
            }
        }
    }
}
