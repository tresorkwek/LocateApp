using System;
using System.Collections.Generic;
using System.Linq;
using LocateApp.Controllers;

namespace LocateApp.Models
{
    /// <summary>Statuts d'une commande d'achat (valeurs stockées en base).</summary>
    public static class StatutCommande
    {
        public const int Brouillon = 0;
        public const int Soumise = 1;
        public const int Validee = 2;
        public const int PartiellementLivree = 3;
        public const int Livree = 4;
        public const int Cloturee = 5;
        public const int Rejetee = 6;

        public static string Libelle(int statut)
        {
            switch (statut)
            {
                case Brouillon: return "Brouillon";
                case Soumise: return "En attente de validation";
                case Validee: return "Validée";
                case PartiellementLivree: return "Partiellement livrée";
                case Livree: return "Livrée";
                case Cloturee: return "Clôturée";
                case Rejetee: return "Rejetée";
                default: return "Inconnu";
            }
        }

        /// <summary>Classe Bootstrap du badge de statut.</summary>
        public static string Badge(int statut)
        {
            switch (statut)
            {
                case Brouillon: return "bg-secondary-subtle text-secondary";
                case Soumise: return "bg-warning-subtle text-warning";
                case Validee: return "bg-primary-subtle text-primary";
                case PartiellementLivree: return "bg-info-subtle text-info";
                case Livree: return "bg-success-subtle text-success";
                case Cloturee: return "bg-dark-subtle text-dark";
                case Rejetee: return "bg-danger-subtle text-danger";
                default: return "bg-secondary-subtle text-secondary";
            }
        }
    }

    public class CommandeAchat
    {
        public long Id { get; set; }
        public string Numero { get; set; }
        public long IdFournisseur { get; set; }
        public string Devise { get; set; }
        /// <summary>Taux figé à la saisie : montant en devise de référence = montant x taux.</summary>
        public decimal Taux { get; set; } = 1;
        public int Statut { get; set; }
        public DateTime DateCommande { get; set; }
        public string Commentaire { get; set; }
        public DateTime DateCreation { get; set; }
        public string UserCreation { get; set; }
        public DateTime? DateSoumission { get; set; }
        public string UserSoumission { get; set; }
        public DateTime? DateValidation { get; set; }
        public string UserValidation { get; set; }
        public string MotifRejet { get; set; }
        public DateTime? DateCloture { get; set; }
        public string UserCloture { get; set; }
        public string MotifCloture { get; set; }

        // Colonnes calculées par les requêtes de liste
        public string Fournisseur { get; set; }
        public int NbreLignes { get; set; }
        public int QuantiteCommandee { get; set; }
        public int QuantiteRecue { get; set; }
        public int QuantiteAnnulee { get; set; }
        public decimal Montant { get; set; }

        public string StatutLibelle => StatutCommande.Libelle(Statut);
        public string StatutBadge => StatutCommande.Badge(Statut);
        public int QuantiteRestante => Math.Max(0, QuantiteCommandee - QuantiteRecue - QuantiteAnnulee);
        public decimal PourcentageRecu => QuantiteCommandee == 0 ? 0 : Math.Round(100m * QuantiteRecue / QuantiteCommandee, 0);
        public decimal MontantReference => Math.Round(Montant * Taux, 2);
        public bool EstEnDeviseReference => Taux == 1m;

        public bool EstModifiable => Statut == StatutCommande.Brouillon || Statut == StatutCommande.Rejetee;
        public bool EstSoumissible => EstModifiable && NbreLignes > 0;
        public bool EstValidable => Statut == StatutCommande.Soumise;
        public bool EstReceptionnable => (Statut == StatutCommande.Validee || Statut == StatutCommande.PartiellementLivree) && QuantiteRestante > 0;
        public bool EstCloturable => EstReceptionnable;
    }

    public class CommandeAchatLigne
    {
        public long Id { get; set; }
        public long IdCommande { get; set; }
        public long IdArticle { get; set; }
        public int Quantite { get; set; }
        public decimal PrixUnitaire { get; set; }
        public int QuantiteRecue { get; set; }
        public int QuantiteAnnulee { get; set; }

        // Colonnes jointes
        public string ArticleCode { get; set; }
        public string ArticleDesignation { get; set; }
        public string ArticleMarque { get; set; }
        public string ArticleModele { get; set; }
        public string UniteMesure { get; set; }

        public int QuantiteRestante => Math.Max(0, Quantite - QuantiteRecue - QuantiteAnnulee);
        public decimal Montant => Quantite * PrixUnitaire;
        public string ArticleComplet => string.Join(" ", new[] { ArticleDesignation, ArticleMarque, ArticleModele }.Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    public class Reception
    {
        public long Id { get; set; }
        public long IdCommande { get; set; }
        public DateTime DateReception { get; set; }
        public long IdLocal { get; set; }
        public string Bordereau { get; set; }
        public string Commentaire { get; set; }
        public DateTime DateCreation { get; set; }
        public string UserCreation { get; set; }

        // Colonnes jointes
        public string NumeroCommande { get; set; }
        public string Fournisseur { get; set; }
        public string LocalCode { get; set; }
        public string LocalDesignation { get; set; }
        public string RecuPar { get; set; }
        public int QuantiteRecue { get; set; }
        public int NbreBiens { get; set; }
    }

    public class ReceptionLigne
    {
        public long Id { get; set; }
        public long IdReception { get; set; }
        public long IdLigneCommande { get; set; }
        public int Quantite { get; set; }

        // Colonnes jointes
        public long IdArticle { get; set; }
        public string ArticleDesignation { get; set; }
        public decimal PrixUnitaire { get; set; }
        public DateTime DateReception { get; set; }
        public string RecuPar { get; set; }
        public int NbreBiens { get; set; }
    }

    public class CommandeAchatPiece
    {
        public Guid Id { get; set; }
        public long IdCommande { get; set; }
        public string NomFichier { get; set; }
        public string Extension { get; set; }
        public string TypePiece { get; set; }
        public DateTime DateCreation { get; set; }
        public string UserCreation { get; set; }

        public string NomStockage => Id.ToString("N") + Extension;
    }

    /// <summary>Historique d'achat d'un article (une ligne par réception).</summary>
    public class AchatArticle
    {
        public long IdCommande { get; set; }
        public string NumeroCommande { get; set; }
        public string Fournisseur { get; set; }
        public DateTime DateReception { get; set; }
        public int Quantite { get; set; }
        public decimal PrixUnitaire { get; set; }
        public string Devise { get; set; }
        public decimal Taux { get; set; } = 1;
        public string RecuPar { get; set; }

        public decimal PrixReference => Math.Round(PrixUnitaire * Taux, 2);
    }
}
