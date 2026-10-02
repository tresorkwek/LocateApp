using System.Collections.Generic;
using System.Linq;
using LocateApp.Controllers;
using LocateApp.Models;

namespace LocateApp.ViewModels
{
    public class OrganigrammeViewModel : ViewModel
    {
        public List<Organe> Organigramme { get; set; }
        public string Link { get; set; }
        public bool ActiveLink { get; set; }
        public bool OrganeParent { get; set; }
        public int AnneEnCours => InventaireController.SelectAnneeEnCours();

        /// <summary>Toutes les versions d'organigramme (sélecteur de version de la vue de configuration).</summary>
        public List<OrganeVersion> Versions { get; set; }

        /// <summary>Version affichée : celle des organes chargés, sinon la version courante.</summary>
        public int VersionAffichee => Organigramme != null && Organigramme.Count > 0 ? Organigramme[0].Version : (VersionCourante?.Id ?? 0);
        public OrganeVersion VersionCourante => Versions?.FirstOrDefault(v => v.Defaut);
        public bool EstVersionCourante => VersionCourante != null && VersionAffichee == VersionCourante.Id;

        public OrganigrammeViewModel(string userName = null, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Link = "/organe/modify/";
            ActiveLink = false;
            Versions = OrganeVersionController.Select();
        }
    }
}
