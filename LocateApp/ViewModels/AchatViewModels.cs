using System.Collections.Generic;
using LocateApp.Controllers;
using LocateApp.Models;

namespace LocateApp.ViewModels
{
    public class DeviseListViewModel : ViewModel
    {
        public List<Devise> Devises { get; set; } = new List<Devise>();

        public DeviseListViewModel(string userName, MessageAlerte messageAlerte = null) : base(userName, messageAlerte) { }
    }

    public class AchatCommandeListViewModel : ViewModel
    {
        public List<CommandeAchat> Commandes { get; set; } = new List<CommandeAchat>();
        public bool AValider { get; set; }
        public string Devise => AchatController.Devise;
        public bool PeutCreer => Utilities.Utilities.CheckClaimStatus(Identity, "GetAchatCommandeAdd");

        public AchatCommandeListViewModel(string userName, MessageAlerte messageAlerte = null) : base(userName, messageAlerte) { }
    }

    public class AchatCommandeFormViewModel : ViewModel
    {
        public CommandeAchat Commande { get; set; }
        public List<CommandeAchatLigne> Lignes { get; set; } = new List<CommandeAchatLigne>();
        public List<Fournisseur> Fournisseurs { get; set; } = new List<Fournisseur>();
        public List<Article> Articles { get; set; } = new List<Article>();
        public List<Devise> Devises { get; set; } = new List<Devise>();
        public bool IsModification => Commande != null;
        public string Devise => Commande?.Devise ?? AchatController.Devise;
        public string DeviseReference => AchatController.Devise;
        public string Link => IsModification ? "/achat/commande/modify/" : "/achat/commande/add/";
        public string CommentaireCommande => Commande?.Commentaire;

        /// <summary>Lignes existantes sérialisées pour le script du formulaire.</summary>
        public string LignesInitialesJson => Newtonsoft.Json.JsonConvert.SerializeObject(
            System.Linq.Enumerable.Select(Lignes, l => new { l.IdArticle, Designation = l.ArticleComplet, l.Quantite, l.PrixUnitaire }));

        public AchatCommandeFormViewModel(string userName, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Fournisseurs = FournisseurController.Select();
            Articles = ArticleController.Select();
            Devises = DeviseController.SelectActives();
        }
    }

    public class AchatCommandeViewModel : ViewModel
    {
        public CommandeAchat Commande { get; set; }
        public List<CommandeAchatLigne> Lignes { get; set; } = new List<CommandeAchatLigne>();
        public List<Reception> Receptions { get; set; } = new List<Reception>();
        public List<CommandeAchatPiece> Pieces { get; set; } = new List<CommandeAchatPiece>();
        public string DeviseReference => AchatController.Devise;

        public bool PeutModifier => Commande.EstModifiable && Utilities.Utilities.CheckClaimStatus(Identity, "GetAchatCommandeModifyId");
        public bool PeutSupprimer => Commande.EstModifiable && Utilities.Utilities.CheckClaimStatus(Identity, "GetAchatCommandeSupprimerId");
        public bool PeutSoumettre => Commande.EstSoumissible && Utilities.Utilities.CheckClaimStatus(Identity, "GetAchatCommandeSoumettreId");
        public bool PeutValider => Commande.EstValidable && Utilities.Utilities.CheckClaimStatus(Identity, "GetAchatCommandeValiderId");
        public bool PeutRejeter => Commande.EstValidable && Utilities.Utilities.CheckClaimStatus(Identity, "PostAchatCommandeRejeter");
        public bool PeutReceptionner => Commande.EstReceptionnable && Utilities.Utilities.CheckClaimStatus(Identity, "GetAchatReceptionAddIdcommande");
        public bool PeutCloturer => Commande.EstCloturable && Utilities.Utilities.CheckClaimStatus(Identity, "PostAchatCommandeCloturer");

        public AchatCommandeViewModel(string userName, MessageAlerte messageAlerte = null) : base(userName, messageAlerte) { }
    }

    public class AchatReceptionFormViewModel : ViewModel
    {
        public CommandeAchat Commande { get; set; }
        public List<CommandeAchatLigne> Lignes { get; set; } = new List<CommandeAchatLigne>();
        public List<Local> Locaux { get; set; } = new List<Local>();

        public AchatReceptionFormViewModel(string userName, MessageAlerte messageAlerte = null) : base(userName, messageAlerte)
        {
            Locaux = LocalController.SelectAll();
        }
    }

    public class AchatReceptionListViewModel : ViewModel
    {
        public List<Reception> Receptions { get; set; } = new List<Reception>();

        public AchatReceptionListViewModel(string userName, MessageAlerte messageAlerte = null) : base(userName, messageAlerte) { }
    }

    public class AchatReceptionViewModel : ViewModel
    {
        public Reception Reception { get; set; }
        public CommandeAchat Commande { get; set; }
        public List<ReceptionLigne> Lignes { get; set; } = new List<ReceptionLigne>();
        public List<Immo> Immos { get; set; } = new List<Immo>();

        public AchatReceptionViewModel(string userName, MessageAlerte messageAlerte = null) : base(userName, messageAlerte) { }
    }
}
