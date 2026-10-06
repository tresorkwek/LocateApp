using System;

namespace LocateApp.Models
{
    /// <summary>Bilan d'une campagne d'inventaire pour un organe (lignes de InventaireDetails regroupées par organe).</summary>
    public class InventaireDetailsOrgane
    {
        public string CodeOrgane { get; set; }
        public string NomOrgane { get; set; }
        public string IdStructure { get; set; }
        public string NomStructure { get; set; }
        public int NbreLignes { get; set; }
        public int NbreLocaux { get; set; }
        public int NbreVus { get; set; }
        public int NbreNonVus { get; set; }
        public int NbreBons { get; set; }
        public int NbreMauvais { get; set; }
        public int NbreInventorieurs { get; set; }
        public DateTime PremierPassage { get; set; }
        public DateTime DernierPassage { get; set; }

        public int TauxPresence => NbreLignes == 0 ? 0 : (int)Math.Round(NbreVus * 100.0 / NbreLignes);
    }
}
