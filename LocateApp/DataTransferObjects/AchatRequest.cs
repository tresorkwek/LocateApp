using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LocateApp.DataTransferObjects
{
    /// <summary>Ligne de commande telle que saisie dans le formulaire (transmise en JSON).</summary>
    [ExcludeFromCodeCoverage]
    public class LigneCommandeAchatRequest
    {
        public long IdArticle { get; set; }
        public int Quantite { get; set; }
        public decimal PrixUnitaire { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class AddCommandeAchatRequest
    {
        public long IdFournisseur { get; set; }
        /// <summary>Code de la devise choisie (table Devise) ; le taux est figé à l'enregistrement.</summary>
        public string Devise { get; set; }
        public DateTime? DateCommande { get; set; }
        public string Commentaire { get; set; }
        /// <summary>Tableau JSON de LigneCommandeAchatRequest.</summary>
        public string LignesJson { get; set; }
        /// <summary>"1" pour soumettre directement après l'enregistrement.</summary>
        public string Soumettre { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ModifyCommandeAchatRequest : AddCommandeAchatRequest
    {
        public long Id { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class RejeterCommandeAchatRequest
    {
        public long Id { get; set; }
        public string Motif { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class CloturerCommandeAchatRequest
    {
        public long Id { get; set; }
        public string Motif { get; set; }
    }

    /// <summary>Quantité reçue pour une ligne de commande (transmise en JSON).</summary>
    [ExcludeFromCodeCoverage]
    public class LigneReceptionRequest
    {
        public long IdLigneCommande { get; set; }
        public int Quantite { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class AddReceptionRequest
    {
        public long IdCommande { get; set; }
        public DateTime? DateReception { get; set; }
        public long IdLocal { get; set; }
        public string Bordereau { get; set; }
        public string Commentaire { get; set; }
        /// <summary>Tableau JSON de LigneReceptionRequest.</summary>
        public string LignesJson { get; set; }
    }
}
