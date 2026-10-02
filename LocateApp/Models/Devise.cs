using System;

namespace LocateApp.Models
{
    /// <summary>Devise utilisable dans les achats. Montant en devise de référence = Montant x Taux.</summary>
    public class Devise
    {
        public string Code { get; set; }
        public string Libelle { get; set; }
        public string Symbole { get; set; }
        public decimal Taux { get; set; }
        public bool EstReference { get; set; }
        public bool Actif { get; set; }
        public DateTime DateMaj { get; set; }
        public string UserMaj { get; set; }

        public string LibelleComplet => string.IsNullOrEmpty(Symbole) ? $"{Code} - {Libelle}" : $"{Code} ({Symbole}) - {Libelle}";
    }
}
