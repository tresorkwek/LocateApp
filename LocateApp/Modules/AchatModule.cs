using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LocateApp.Controllers;
using LocateApp.DataTransferObjects;
using LocateApp.Models;
using LocateApp.Utilities;
using LocateApp.ViewModels;
using Nancy;
using Nancy.Security;
using Newtonsoft.Json;

namespace LocateApp.Modules
{
    /// <summary>Commandes d'achat (saisie, validation à deux niveaux, clôture) et réceptions de livraison.</summary>
    public class AchatModule : NancyModule
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(AchatModule));
        private const string DossierPieces = "Content/documents/achats";

        public AchatModule() : base("/achat")
        {
            this.RequiresAuthentication();

            Get("/commande/", _ => this.RunHandler(GetCommandes));
            Get("/commande/avalider/", _ => this.RunHandler(GetCommandesAValider));
            Get("/commande/add/", _ => this.RunHandler(GetAddCommandeForm));
            Get("/commande/id/{id}", _ => this.RunHandler<long>(GetCommandeById, (long)_.id));
            Get("/commande/modify/{id}", _ => this.RunHandler<long>(GetModifyCommandeForm, (long)_.id));
            Get("/commande/soumettre/{id}", _ => this.RunHandler<long>(SoumettreCommande, (long)_.id));
            Get("/commande/valider/{id}", _ => this.RunHandler<long>(ValiderCommande, (long)_.id));
            Get("/commande/supprimer/{id}", _ => this.RunHandler<long>(SupprimerCommande, (long)_.id));
            Get("/commande/piece/{id}", _ => this.RunHandler<Guid>(GetPiece, (Guid)_.id));

            Post("/commande/add/", _ => this.RunHandler<AddCommandeAchatRequest>(AddCommande));
            Post("/commande/modify/", _ => this.RunHandler<ModifyCommandeAchatRequest>(ModifyCommande));
            Post("/commande/rejeter/", _ => this.RunHandler<RejeterCommandeAchatRequest>(RejeterCommande));
            Post("/commande/cloturer/", _ => this.RunHandler<CloturerCommandeAchatRequest>(CloturerCommande));

            Get("/reception/", _ => this.RunHandler(GetReceptions));
            Get("/reception/add/{idCommande}", _ => this.RunHandler<long>(GetAddReceptionForm, (long)_.idCommande));
            Get("/reception/id/{id}", _ => this.RunHandler<long>(GetReceptionById, (long)_.id));

            Post("/reception/add/", _ => this.RunHandler<AddReceptionRequest>(AddReception));
        }

        // ------------------------------------------------------------------ Listes

        private object GetCommandes()
        {
            AchatCommandeListViewModel viewModel = new AchatCommandeListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Commandes = AchatController.SelectCommandes()
            };

            object view = View["AchatCommandeListView", viewModel];
            return this.ResponseObject(view, viewModel.Commandes, viewModel.Title, viewModel.Commandes.Count > 0 ? 1 : 0);
        }

        private object GetCommandesAValider()
        {
            AchatCommandeListViewModel viewModel = new AchatCommandeListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Commandes = AchatController.SelectCommandesByStatut(StatutCommande.Soumise),
                AValider = true
            };

            object view = View["AchatCommandeListView", viewModel];
            return this.ResponseObject(view, viewModel.Commandes, viewModel.Title, viewModel.Commandes.Count > 0 ? 1 : 0);
        }

        private object GetReceptions()
        {
            AchatReceptionListViewModel viewModel = new AchatReceptionListViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Receptions = AchatController.SelectReceptions()
            };

            object view = View["AchatReceptionListView", viewModel];
            return this.ResponseObject(view, viewModel.Receptions, viewModel.Title, viewModel.Receptions.Count > 0 ? 1 : 0);
        }

        // ------------------------------------------------------------------ Détail

        private object GetCommandeById(long id)
        {
            CommandeAchat commande = AchatController.SelectCommandeById(id);
            if (commande == null) return HttpStatusCode.NotFound;

            AchatCommandeViewModel viewModel = new AchatCommandeViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Commande = commande,
                Lignes = AchatController.SelectLignes(id),
                Receptions = AchatController.SelectReceptionsByCommande(id),
                Pieces = AchatController.SelectPieces(id)
            };

            object view = View["AchatCommandeView", viewModel];
            return this.ResponseObject(view, commande, viewModel.Title, 1);
        }

        private object GetReceptionById(long id)
        {
            Reception reception = AchatController.SelectReceptionById(id);
            if (reception == null) return HttpStatusCode.NotFound;

            AchatReceptionViewModel viewModel = new AchatReceptionViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Reception = reception,
                Commande = AchatController.SelectCommandeById(reception.IdCommande),
                Lignes = AchatController.SelectReceptionLignes(id),
                Immos = AchatController.SelectImmosByReception(id)
            };

            object view = View["AchatReceptionView", viewModel];
            return this.ResponseObject(view, reception, viewModel.Title, 1);
        }

        // ------------------------------------------------------------------ Saisie de commande

        private object GetAddCommandeForm()
        {
            AchatCommandeFormViewModel viewModel = new AchatCommandeFormViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString()
            };

            object view = View["AchatCommandeFormView", viewModel];
            return this.ResponseObject(view, null, viewModel.Title, 1);
        }

        private object GetModifyCommandeForm(long id)
        {
            CommandeAchat commande = AchatController.SelectCommandeById(id);
            if (commande == null) return HttpStatusCode.NotFound;

            if (!commande.EstModifiable)
            {
                return this.RedirectUrl($"/achat/commande/id/{id}", $"La commande {commande.Numero} n'est plus modifiable ({commande.StatutLibelle}).", 2, "Modification impossible");
            }

            AchatCommandeFormViewModel viewModel = new AchatCommandeFormViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Commande = commande,
                Lignes = AchatController.SelectLignes(id)
            };

            object view = View["AchatCommandeFormView", viewModel];
            return this.ResponseObject(view, commande, viewModel.Title, 1);
        }

        private object AddCommande(AddCommandeAchatRequest values)
        {
            Identity user = IdentityController.GetUser(this.CurrentUserName());
            List<LigneCommandeAchatRequest> lignes = LireLignes(values.LignesJson, out string erreur);

            if (erreur == null && values.IdFournisseur <= 0) erreur = "Le fournisseur est obligatoire.";
            if (erreur == null && AchatController.ResoudreDevise(values.Devise) == null) erreur = "Choisissez une devise active.";
            if (erreur != null)
            {
                return this.RedirectUrl("/achat/commande/add/", erreur, 0, "Commande d'achat");
            }

            long id = AchatController.InsertCommande(values, lignes, user);
            if (id == 0)
            {
                return this.RedirectUrl("/achat/commande/add/", "L'enregistrement de la commande a échoué.", 0, "Commande d'achat");
            }

            CommandeAchat commande = AchatController.SelectCommandeById(id);
            string message = $"Commande {commande.Numero} enregistrée en brouillon.";

            if (values.Soumettre == "1" && AchatController.Soumettre(id, user))
            {
                message = $"Commande {commande.Numero} enregistrée et soumise à validation.";
            }

            return this.RedirectUrl($"/achat/commande/id/{id}", message, 1, "Commande d'achat");
        }

        private object ModifyCommande(ModifyCommandeAchatRequest values)
        {
            Identity user = IdentityController.GetUser(this.CurrentUserName());
            List<LigneCommandeAchatRequest> lignes = LireLignes(values.LignesJson, out string erreur);

            if (erreur == null && values.IdFournisseur <= 0) erreur = "Le fournisseur est obligatoire.";
            if (erreur == null && AchatController.ResoudreDevise(values.Devise) == null) erreur = "Choisissez une devise active.";
            if (erreur != null)
            {
                return this.RedirectUrl($"/achat/commande/modify/{values.Id}", erreur, 0, "Commande d'achat");
            }

            bool result = AchatController.UpdateCommande(values, lignes, user);
            CommandeAchat commande = AchatController.SelectCommandeById(values.Id);

            if (!result)
            {
                return this.RedirectUrl($"/achat/commande/id/{values.Id}", "La modification de la commande a échoué.", 0, "Commande d'achat");
            }

            string message = $"Commande {commande.Numero} modifiée.";
            if (values.Soumettre == "1" && AchatController.Soumettre(values.Id, user))
            {
                message = $"Commande {commande.Numero} modifiée et soumise à validation.";
            }

            return this.RedirectUrl($"/achat/commande/id/{values.Id}", message, 1, "Commande d'achat");
        }

        private object SupprimerCommande(long id)
        {
            CommandeAchat commande = AchatController.SelectCommandeById(id);
            if (commande == null) return HttpStatusCode.NotFound;

            if (!commande.EstModifiable)
            {
                return this.RedirectUrl($"/achat/commande/id/{id}", "Seule une commande en brouillon ou rejetée peut être supprimée.", 2, "Suppression");
            }

            bool result = AchatController.DeleteCommande(id, IdentityController.GetUser(this.CurrentUserName()));
            string message = result ? $"Commande {commande.Numero} supprimée." : "La suppression a échoué.";

            return this.RedirectUrl("/achat/commande/", message, result ? 1 : 0, "Suppression");
        }

        // ------------------------------------------------------------------ Circuit de validation

        private object SoumettreCommande(long id)
        {
            CommandeAchat commande = AchatController.SelectCommandeById(id);
            if (commande == null) return HttpStatusCode.NotFound;

            bool result = commande.EstSoumissible && AchatController.Soumettre(id, IdentityController.GetUser(this.CurrentUserName()));
            string message = result ? $"Commande {commande.Numero} soumise à validation." : "La commande ne peut pas être soumise : vérifiez qu'elle contient au moins une ligne.";

            return this.RedirectUrl($"/achat/commande/id/{id}", message, result ? 1 : 0, "Soumission");
        }

        private object ValiderCommande(long id)
        {
            CommandeAchat commande = AchatController.SelectCommandeById(id);
            if (commande == null) return HttpStatusCode.NotFound;

            Identity user = IdentityController.GetUser(this.CurrentUserName());
            bool result = commande.EstValidable && AchatController.Valider(id, user);
            string message = result ? $"Commande {commande.Numero} validée : elle peut maintenant être réceptionnée." : "Cette commande n'est pas en attente de validation.";

            return this.RedirectUrl($"/achat/commande/id/{id}", message, result ? 1 : 0, "Validation");
        }

        private object RejeterCommande(RejeterCommandeAchatRequest values)
        {
            CommandeAchat commande = AchatController.SelectCommandeById(values.Id);
            if (commande == null) return HttpStatusCode.NotFound;

            if (string.IsNullOrWhiteSpace(values.Motif))
            {
                return this.RedirectUrl($"/achat/commande/id/{values.Id}", "Le motif du rejet est obligatoire.", 0, "Rejet");
            }

            bool result = commande.EstValidable && AchatController.Rejeter(values.Id, values.Motif.Trim(), IdentityController.GetUser(this.CurrentUserName()));
            string message = result ? $"Commande {commande.Numero} renvoyée au demandeur." : "Cette commande n'est pas en attente de validation.";

            return this.RedirectUrl($"/achat/commande/id/{values.Id}", message, result ? 1 : 0, "Rejet");
        }

        private object CloturerCommande(CloturerCommandeAchatRequest values)
        {
            CommandeAchat commande = AchatController.SelectCommandeById(values.Id);
            if (commande == null) return HttpStatusCode.NotFound;

            Identity user = IdentityController.GetUser(this.CurrentUserName());
            List<HttpFile> fichiers = this.Request.Files?.Where(f => f.Value != null && f.Value.Length > 0).ToList() ?? new List<HttpFile>();

            if (!commande.EstCloturable)
            {
                return this.RedirectUrl($"/achat/commande/id/{values.Id}", "Seule une commande validée avec un reliquat peut être clôturée.", 2, "Clôture");
            }
            if (string.IsNullOrWhiteSpace(values.Motif))
            {
                return this.RedirectUrl($"/achat/commande/id/{values.Id}", "Le motif de clôture est obligatoire.", 0, "Clôture");
            }
            if (fichiers.Count == 0)
            {
                return this.RedirectUrl($"/achat/commande/id/{values.Id}", "Joignez au moins une pièce justificative (courrier du fournisseur, preuve d'annulation...).", 0, "Clôture");
            }

            // Pièces justificatives d'abord : sans elles, pas de clôture.
            int piecesEnregistrees = 0;
            foreach (HttpFile fichier in fichiers)
            {
                if (EnregistrerPiece(fichier, values.Id, "Cloture", user)) piecesEnregistrees++;
            }

            if (piecesEnregistrees == 0)
            {
                return this.RedirectUrl($"/achat/commande/id/{values.Id}", "Les pièces justificatives n'ont pas pu être enregistrées.", 0, "Clôture");
            }

            bool result = AchatController.Cloturer(values.Id, values.Motif.Trim(), user);
            string message = result ? $"Commande {commande.Numero} clôturée : le reliquat de {commande.QuantiteRestante} unité(s) est annulé." : "La clôture a échoué.";

            return this.RedirectUrl($"/achat/commande/id/{values.Id}", message, result ? 1 : 0, "Clôture");
        }

        // ------------------------------------------------------------------ Réceptions

        private object GetAddReceptionForm(long idCommande)
        {
            CommandeAchat commande = AchatController.SelectCommandeById(idCommande);
            if (commande == null) return HttpStatusCode.NotFound;

            if (!commande.EstReceptionnable)
            {
                return this.RedirectUrl($"/achat/commande/id/{idCommande}", $"La commande {commande.Numero} ne peut pas être réceptionnée ({commande.StatutLibelle}).", 2, "Réception");
            }

            AchatReceptionFormViewModel viewModel = new AchatReceptionFormViewModel(this.CurrentUserName(), this.ShowAlert())
            {
                MenuData = MenuController.GetMenuByName(this.GetClaimString()),
                CurrentClaim = this.GetClaimString(),
                Commande = commande,
                Lignes = AchatController.SelectLignes(idCommande)
            };

            object view = View["AchatReceptionFormView", viewModel];
            return this.ResponseObject(view, commande, viewModel.Title, 1);
        }

        private object AddReception(AddReceptionRequest values)
        {
            Identity user = IdentityController.GetUser(this.CurrentUserName());
            string retour = $"/achat/reception/add/{values.IdCommande}";

            if (values.IdLocal <= 0)
            {
                return this.RedirectUrl(retour, "Le local de réception est obligatoire.", 0, "Réception");
            }

            List<LigneReceptionRequest> lignes;
            try
            {
                lignes = JsonConvert.DeserializeObject<List<LigneReceptionRequest>>(values.LignesJson ?? "[]") ?? new List<LigneReceptionRequest>();
            }
            catch (Exception e)
            {
                Log.Error(user, $"Réception : lignes illisibles ({e.Message}) : {values.LignesJson}");
                return this.RedirectUrl(retour, "Les quantités reçues sont illisibles.", 0, "Réception");
            }

            long idReception = AchatController.InsertReception(values, lignes, user, out int nbreBiens, out string erreur);
            if (idReception == 0)
            {
                return this.RedirectUrl(retour, erreur ?? "L'enregistrement de la réception a échoué.", 0, "Réception");
            }

            CommandeAchat commande = AchatController.SelectCommandeById(values.IdCommande);
            string message = $"Réception enregistrée : {nbreBiens} bien(s) créé(s). Commande {commande.Numero} : {commande.StatutLibelle.ToLower()}.";

            return this.RedirectUrl($"/achat/reception/id/{idReception}", message, 1, "Réception");
        }

        // ------------------------------------------------------------------ Pièces jointes

        private object GetPiece(Guid id)
        {
            CommandeAchatPiece piece = AchatController.SelectPieceById(id);
            if (piece == null) return HttpStatusCode.NotFound;

            string chemin = $"{DossierPieces}/{piece.IdCommande}/{piece.NomStockage}";
            string cheminPhysique = Path.Combine(new DefaultRootPathProvider().GetRootPath(), chemin.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(cheminPhysique)) return HttpStatusCode.NotFound;

            return Response.AsFile(chemin, MimeTypes.GetMimeType(piece.NomFichier))
                           .WithHeader("Content-Disposition", $"attachment; filename=\"{piece.NomFichier}\"");
        }

        private bool EnregistrerPiece(HttpFile fichier, long idCommande, string type, Identity user)
        {
            try
            {
                string extension = Path.GetExtension(fichier.Name)?.ToLowerInvariant() ?? "";
                if (extension.Length > 10) extension = extension.Substring(0, 10);

                CommandeAchatPiece piece = new CommandeAchatPiece
                {
                    Id = Guid.NewGuid(),
                    IdCommande = idCommande,
                    NomFichier = Path.GetFileName(fichier.Name),
                    Extension = extension,
                    TypePiece = type
                };

                string dossier = Path.Combine(new DefaultRootPathProvider().GetRootPath(), DossierPieces.Replace('/', Path.DirectorySeparatorChar), idCommande.ToString());
                Directory.CreateDirectory(dossier);

                using (var flux = new FileStream(Path.Combine(dossier, piece.NomStockage), FileMode.Create))
                {
                    fichier.Value.CopyTo(flux);
                }

                return AchatController.InsertPiece(piece, user);
            }
            catch (Exception e)
            {
                Log.Error(user, $"Enregistrement de la pièce jointe '{fichier.Name}' impossible (commande {idCommande}) : {e.Message}");
                return false;
            }
        }

        // ------------------------------------------------------------------ Outils

        private List<LigneCommandeAchatRequest> LireLignes(string json, out string erreur)
        {
            erreur = null;
            List<LigneCommandeAchatRequest> lignes;

            try
            {
                lignes = JsonConvert.DeserializeObject<List<LigneCommandeAchatRequest>>(json ?? "[]") ?? new List<LigneCommandeAchatRequest>();
            }
            catch (Exception e)
            {
                Log.Error(IdentityController.GetUser(this.CurrentUserName()), $"Commande d'achat : lignes illisibles ({e.Message}) : {json}");
                erreur = "Les lignes de la commande sont illisibles.";
                return new List<LigneCommandeAchatRequest>();
            }

            lignes = lignes.Where(l => l.IdArticle > 0 && l.Quantite > 0).ToList();
            if (lignes.Count == 0) erreur = "Ajoutez au moins une ligne avec un article et une quantité.";
            if (lignes.Any(l => l.PrixUnitaire < 0)) erreur = "Un prix unitaire ne peut pas être négatif.";

            return lignes;
        }
    }
}
