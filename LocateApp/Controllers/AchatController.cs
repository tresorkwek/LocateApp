using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Dapper;
using LocateApp.DataTransferObjects;
using LocateApp.Models;
using LocateApp.Repository;
using LocateApp.Utilities;

namespace LocateApp.Controllers
{
    /// <summary>Commandes d'achat, réceptions et création des biens reçus.</summary>
    public static class AchatController
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(AchatController));

        /// <summary>Code de la devise de référence (taux 1) définie dans la table Devise.</summary>
        public static string Devise => DeviseController.Reference().Code;

        /// <summary>Devise active demandée pour une commande ; null si le code est inconnu ou inactif.</summary>
        public static Devise ResoudreDevise(string code)
        {
            Devise devise = DeviseController.SelectByCode(code);
            return devise != null && devise.Actif ? devise : null;
        }

        // ------------------------------------------------------------------ Commandes

        public static List<CommandeAchat> SelectCommandes()
        {
            return SqlDataAccess.SelectData<CommandeAchat>(SqlAchat.SelectCommandes) ?? new List<CommandeAchat>();
        }

        public static List<CommandeAchat> SelectCommandesByStatut(int statut)
        {
            return SqlDataAccess.SelectData<CommandeAchat>(SqlAchat.SelectCommandesByStatut, new { Statut = statut }) ?? new List<CommandeAchat>();
        }

        public static CommandeAchat SelectCommandeById(long id)
        {
            return SqlDataAccess.SelectData<CommandeAchat>(SqlAchat.SelectCommandeById, new { Id = id })?.FirstOrDefault();
        }

        public static List<CommandeAchatLigne> SelectLignes(long idCommande)
        {
            return SqlDataAccess.SelectData<CommandeAchatLigne>(SqlAchat.SelectLignes, new { IdCommande = idCommande }) ?? new List<CommandeAchatLigne>();
        }

        /// <summary>Crée la commande et ses lignes dans une même transaction. Retourne l'identifiant créé, 0 en cas d'échec.</summary>
        public static long InsertCommande(AddCommandeAchatRequest commande, List<LigneCommandeAchatRequest> lignes, Identity user)
        {
            long id = 0;
            Devise devise = ResoudreDevise(commande.Devise) ?? DeviseController.Reference();

            using (IDbConnection connection = SqlDataAccess.GetConnexion())
            {
                try
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        string numero = connection.QuerySingle<string>(SqlAchat.SelectNextNumero, transaction: transaction);

                        id = connection.QuerySingle<long>(SqlAchat.InsertCommande, new
                        {
                            Numero = numero,
                            commande.IdFournisseur,
                            Devise = devise.Code,
                            devise.Taux,
                            Statut = StatutCommande.Brouillon,
                            DateCommande = commande.DateCommande ?? DateTime.Today,
                            commande.Commentaire,
                            UserCreation = user.UserName
                        }, transaction);

                        foreach (var ligne in lignes)
                        {
                            connection.Execute(SqlAchat.InsertLigne, new { IdCommande = id, ligne.IdArticle, ligne.Quantite, ligne.PrixUnitaire }, transaction);
                        }

                        transaction.Commit();
                    }
                }
                catch (Exception e)
                {
                    id = 0;
                    Log.Error(user, $"Création de la commande d'achat impossible : {e.Message}");
                }
            }

            return id;
        }

        /// <summary>Remplace l'en-tête et les lignes d'une commande encore modifiable.</summary>
        public static bool UpdateCommande(ModifyCommandeAchatRequest commande, List<LigneCommandeAchatRequest> lignes, Identity user)
        {
            bool result = false;
            Devise devise = ResoudreDevise(commande.Devise) ?? DeviseController.Reference();

            using (IDbConnection connection = SqlDataAccess.GetConnexion())
            {
                try
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        int modifie = connection.Execute(SqlAchat.UpdateCommande, new
                        {
                            commande.Id,
                            commande.IdFournisseur,
                            Devise = devise.Code,
                            devise.Taux,
                            DateCommande = commande.DateCommande ?? DateTime.Today,
                            commande.Commentaire
                        }, transaction);

                        if (modifie == 1)
                        {
                            connection.Execute(SqlAchat.DeleteLignes, new { IdCommande = commande.Id }, transaction);

                            foreach (var ligne in lignes)
                            {
                                connection.Execute(SqlAchat.InsertLigne, new { IdCommande = commande.Id, ligne.IdArticle, ligne.Quantite, ligne.PrixUnitaire }, transaction);
                            }

                            transaction.Commit();
                            result = true;
                        }
                        else
                        {
                            transaction.Rollback();
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(user, $"Modification de la commande d'achat {commande.Id} impossible : {e.Message}");
                }
            }

            return result;
        }

        public static bool DeleteCommande(long id, Identity user)
        {
            var operations = new List<(string, object)>
            {
                (SqlAchat.DeleteLignes, new { IdCommande = id }),
                (SqlAchat.DeleteCommande, new { Id = id })
            };

            return SqlDataAccess.SaveDataWithTransaction(operations, user) > 0;
        }

        public static bool Soumettre(long id, Identity user)
        {
            return SqlDataAccess.SaveData(SqlAchat.Soumettre, new { Id = id, user.UserName }, user) == 1;
        }

        public static bool Valider(long id, Identity user)
        {
            return SqlDataAccess.SaveData(SqlAchat.Valider, new { Id = id, user.UserName }, user) == 1;
        }

        public static bool Rejeter(long id, string motif, Identity user)
        {
            return SqlDataAccess.SaveData(SqlAchat.Rejeter, new { Id = id, Motif = motif, user.UserName }, user) == 1;
        }

        /// <summary>Annule le reliquat non livré et clôture la commande.</summary>
        public static bool Cloturer(long id, string motif, Identity user)
        {
            var operations = new List<(string, object)>
            {
                (SqlAchat.AnnulerReliquats, new { Id = id }),
                (SqlAchat.Cloturer, new { Id = id, Motif = motif, user.UserName })
            };

            return SqlDataAccess.SaveDataWithTransaction(operations, user) > 0;
        }

        // ------------------------------------------------------------------ Réceptions

        public static List<Reception> SelectReceptions()
        {
            return SqlDataAccess.SelectData<Reception>(SqlAchat.SelectReceptions) ?? new List<Reception>();
        }

        public static List<Reception> SelectReceptionsByCommande(long idCommande)
        {
            return SqlDataAccess.SelectData<Reception>(SqlAchat.SelectReceptionsByCommande, new { IdCommande = idCommande }) ?? new List<Reception>();
        }

        public static Reception SelectReceptionById(long id)
        {
            return SqlDataAccess.SelectData<Reception>(SqlAchat.SelectReceptionById, new { Id = id })?.FirstOrDefault();
        }

        public static List<ReceptionLigne> SelectReceptionLignes(long idReception)
        {
            return SqlDataAccess.SelectData<ReceptionLigne>(SqlAchat.SelectReceptionLignes, new { IdReception = idReception }) ?? new List<ReceptionLigne>();
        }

        public static List<Immo> SelectImmosByReception(long idReception)
        {
            return SqlDataAccess.SelectData<Immo>(SqlAchat.SelectImmosByReception, new { IdReception = idReception }) ?? new List<Immo>();
        }

        /// <summary>
        /// Enregistre une livraison (totale ou partielle), met à jour les quantités reçues et le statut de la commande,
        /// puis crée un bien par unité reçue dans le local indiqué. Retourne l'identifiant de la réception, 0 en cas d'échec.
        /// </summary>
        public static long InsertReception(AddReceptionRequest reception, List<LigneReceptionRequest> lignesRecues, Identity user, out int nbreBiensCrees, out string erreur)
        {
            nbreBiensCrees = 0;
            erreur = null;

            CommandeAchat commande = SelectCommandeById(reception.IdCommande);
            if (commande == null || !commande.EstReceptionnable)
            {
                erreur = "Cette commande ne peut pas recevoir de livraison.";
                return 0;
            }

            List<CommandeAchatLigne> lignesCommande = SelectLignes(reception.IdCommande);
            var aRecevoir = new List<(CommandeAchatLigne ligne, int quantite)>();

            foreach (var recue in lignesRecues.Where(l => l.Quantite > 0))
            {
                CommandeAchatLigne ligne = lignesCommande.FirstOrDefault(l => l.Id == recue.IdLigneCommande);
                if (ligne == null)
                {
                    erreur = "Une ligne reçue ne correspond pas à la commande.";
                    return 0;
                }
                if (recue.Quantite > ligne.QuantiteRestante)
                {
                    erreur = $"Quantité reçue supérieure au reste à livrer pour {ligne.ArticleDesignation} ({ligne.QuantiteRestante} restant).";
                    return 0;
                }
                aRecevoir.Add((ligne, recue.Quantite));
            }

            if (aRecevoir.Count == 0)
            {
                erreur = "Aucune quantité reçue n'a été saisie.";
                return 0;
            }

            long idReception = 0;
            var lignesCreees = new List<(long idReceptionLigne, CommandeAchatLigne ligne, int quantite)>();

            using (IDbConnection connection = SqlDataAccess.GetConnexion())
            {
                try
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        idReception = connection.QuerySingle<long>(SqlAchat.InsertReception, new
                        {
                            reception.IdCommande,
                            DateReception = reception.DateReception ?? DateTime.Today,
                            reception.IdLocal,
                            reception.Bordereau,
                            reception.Commentaire,
                            UserCreation = user.UserName
                        }, transaction);

                        foreach (var (ligne, quantite) in aRecevoir)
                        {
                            long idLigne = connection.QuerySingle<long>(SqlAchat.InsertReceptionLigne, new { IdReception = idReception, IdLigneCommande = ligne.Id, Quantite = quantite }, transaction);
                            lignesCreees.Add((idLigne, ligne, quantite));
                        }

                        connection.Execute(SqlAchat.RecalculerStatut, new { Id = reception.IdCommande }, transaction);
                        transaction.Commit();
                    }
                }
                catch (Exception e)
                {
                    Log.Error(user, $"Enregistrement de la réception impossible (commande {reception.IdCommande}) : {e.Message}");
                    erreur = "L'enregistrement de la réception a échoué.";
                    return 0;
                }
            }

            // Création des biens : un par unité reçue, dans le local de réception.
            int idObservation = ObservationNouvelleAcquisition();
            DateTime dateAcquisition = reception.DateReception ?? DateTime.Today;

            foreach (var (idReceptionLigne, ligne, quantite) in lignesCreees)
            {
                for (int i = 0; i < quantite; i++)
                {
                    var immo = new InsertImmoRequest
                    {
                        IdArticle = ligne.IdArticle,
                        IdLocal = reception.IdLocal,
                        LastEtat = "B",
                        IdLastObservation = idObservation,
                        Inventaire = false,
                        Nbre = 1
                    };

                    if (ImmoController.InsertImmo(immo, user, out long idImmo))
                    {
                        SqlDataAccess.SaveData(SqlAchat.UpdateImmoAcquisition, new
                        {
                            Id = idImmo,
                            IdReceptionLigne = idReceptionLigne,
                            PrixAcquisition = ligne.PrixUnitaire,
                            DateAcquisition = dateAcquisition,
                            DeviseAcquisition = commande.Devise,
                            TauxAcquisition = commande.Taux
                        }, user);
                        nbreBiensCrees++;
                    }
                    else
                    {
                        Log.Error(user, $"Réception {idReception} : création du bien impossible pour l'article {ligne.IdArticle} (ligne {idReceptionLigne}).");
                    }
                }
            }

            return idReception;
        }

        /// <summary>Observation par défaut des biens reçus : "nouvelle acquisition" si elle existe, sinon la première observation de l'état Bon.</summary>
        private static int ObservationNouvelleAcquisition()
        {
            List<Observations> observations = ObservationsController.SelectByEtat("B") ?? new List<Observations>();
            Observations acquisition = observations.FirstOrDefault(o => (o.Observation ?? "").IndexOf("acquisition", StringComparison.OrdinalIgnoreCase) >= 0)
                                       ?? observations.FirstOrDefault();
            return acquisition?.Id ?? 0;
        }

        // ------------------------------------------------------------------ Pièces jointes

        public static bool InsertPiece(CommandeAchatPiece piece, Identity user)
        {
            return SqlDataAccess.SaveData(SqlAchat.InsertPiece, new { piece.Id, piece.IdCommande, piece.NomFichier, piece.Extension, piece.TypePiece, UserCreation = user.UserName }, user) == 1;
        }

        public static List<CommandeAchatPiece> SelectPieces(long idCommande)
        {
            return SqlDataAccess.SelectData<CommandeAchatPiece>(SqlAchat.SelectPiecesByCommande, new { IdCommande = idCommande }) ?? new List<CommandeAchatPiece>();
        }

        public static CommandeAchatPiece SelectPieceById(Guid id)
        {
            return SqlDataAccess.SelectData<CommandeAchatPiece>(SqlAchat.SelectPieceById, new { Id = id })?.FirstOrDefault();
        }

        // ------------------------------------------------------------------ Historique par article

        public static List<AchatArticle> SelectAchatsByArticle(long idArticle)
        {
            return SqlDataAccess.SelectData<AchatArticle>(SqlAchat.SelectAchatsByArticle, new { IdArticle = idArticle }) ?? new List<AchatArticle>();
        }

        public static decimal? SelectDernierPrix(long idArticle)
        {
            return SqlDataAccess.SelectData<decimal>(SqlAchat.SelectDernierPrixArticle, new { IdArticle = idArticle })?.Cast<decimal?>().FirstOrDefault();
        }
    }
}
