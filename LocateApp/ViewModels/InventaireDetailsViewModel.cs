using System.Collections.Generic;
using LocateApp.Controllers;
using LocateApp.Models;

namespace LocateApp.ViewModels
{
    /// <summary>
    /// Détail d'une campagne d'inventaire : bilan par organe, ou lignes d'un organe quand <see cref="Organe"/> est renseigné.
    /// </summary>
    public class InventaireDetailsViewModel : ViewModel
    {
        public int Annee { get; set; }
        public Inventaire Inventaire { get; set; }
        public List<InventaireDetailsOrgane> Organes { get; set; } = new List<InventaireDetailsOrgane>();
        public Organe Organe { get; set; }
        public List<InventaireDetails> Lignes { get; set; } = new List<InventaireDetails>();
        public int AnneeEnCours => InventaireController.SelectAnneeEnCours();
        public string Link { get; set; }

        public InventaireDetailsViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/inventaire/details/";
        }
    }
}
