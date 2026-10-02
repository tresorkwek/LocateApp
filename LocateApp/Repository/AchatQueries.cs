namespace LocateApp.Repository
{
    public static class SqlAchat
    {
        private const string ColonnesCommande = @"c.Id, c.Numero, c.IdFournisseur, c.Devise, c.Taux, c.Statut, c.DateCommande, c.Commentaire, c.DateCreation, c.UserCreation,
                                                  c.DateSoumission, c.UserSoumission, c.DateValidation, c.UserValidation, c.MotifRejet, c.DateCloture, c.UserCloture, c.MotifCloture,
                                                  f.Nom AS Fournisseur,
                                                  ISNULL(l.NbreLignes, 0) AS NbreLignes, ISNULL(l.QuantiteCommandee, 0) AS QuantiteCommandee,
                                                  ISNULL(l.QuantiteRecue, 0) AS QuantiteRecue, ISNULL(l.QuantiteAnnulee, 0) AS QuantiteAnnulee, ISNULL(l.Montant, 0) AS Montant";

        private const string JointuresCommande = @"FROM CommandeAchat c
                                                   INNER JOIN ArticleFournisseur f ON f.Id = c.IdFournisseur
                                                   LEFT JOIN (SELECT IdCommande, COUNT(*) AS NbreLignes, SUM(Quantite) AS QuantiteCommandee, SUM(QuantiteRecue) AS QuantiteRecue,
                                                                     SUM(QuantiteAnnulee) AS QuantiteAnnulee, SUM(Quantite * PrixUnitaire) AS Montant
                                                              FROM CommandeAchatLigne GROUP BY IdCommande) l ON l.IdCommande = c.Id";

        public static string SelectCommandes { get; } = "SELECT " + ColonnesCommande + " " + JointuresCommande + " ORDER BY c.DateCreation DESC";

        public static string SelectCommandesByStatut { get; } = "SELECT " + ColonnesCommande + " " + JointuresCommande + " WHERE c.Statut = @Statut ORDER BY c.DateSoumission, c.DateCreation";

        public static string SelectCommandeById { get; } = "SELECT " + ColonnesCommande + " " + JointuresCommande + " WHERE c.Id = @Id";

        public static string SelectNextNumero { get; } = @"SELECT 'BC-' + FORMAT(GETDATE(), 'yyyyMM') + '-' + RIGHT('0000' + CAST(ISNULL(MAX(CAST(RIGHT(Numero, 4) AS int)), 0) + 1 AS varchar), 4)
                                                          FROM CommandeAchat
                                                          WHERE Numero LIKE 'BC-' + FORMAT(GETDATE(), 'yyyyMM') + '-%'";

        public static string InsertCommande { get; } = @"INSERT INTO CommandeAchat (Numero, IdFournisseur, Devise, Taux, Statut, DateCommande, Commentaire, UserCreation)
                                                        VALUES (@Numero, @IdFournisseur, @Devise, @Taux, @Statut, @DateCommande, @Commentaire, @UserCreation);
                                                        SELECT CAST(SCOPE_IDENTITY() AS bigint)";

        public static string UpdateCommande { get; } = @"UPDATE CommandeAchat
                                                        SET IdFournisseur = @IdFournisseur, Devise = @Devise, Taux = @Taux, DateCommande = @DateCommande, Commentaire = @Commentaire
                                                        WHERE Id = @Id AND Statut IN (0, 6)";

        public static string DeleteLignes { get; } = @"DELETE FROM CommandeAchatLigne WHERE IdCommande = @IdCommande";

        public static string DeleteCommande { get; } = @"DELETE FROM CommandeAchat WHERE Id = @Id AND Statut IN (0, 6)";

        public static string InsertLigne { get; } = @"INSERT INTO CommandeAchatLigne (IdCommande, IdArticle, Quantite, PrixUnitaire)
                                                     VALUES (@IdCommande, @IdArticle, @Quantite, @PrixUnitaire)";

        public static string SelectLignes { get; } = @"SELECT l.Id, l.IdCommande, l.IdArticle, l.Quantite, l.PrixUnitaire, l.QuantiteRecue, l.QuantiteAnnulee,
                                                             a.Code AS ArticleCode, a.Designation AS ArticleDesignation, a.Marque AS ArticleMarque, a.Modele AS ArticleModele, a.UniteMesure
                                                      FROM CommandeAchatLigne l INNER JOIN Article a ON a.Id = l.IdArticle
                                                      WHERE l.IdCommande = @IdCommande
                                                      ORDER BY l.Id";

        public static string SelectLigneById { get; } = @"SELECT l.Id, l.IdCommande, l.IdArticle, l.Quantite, l.PrixUnitaire, l.QuantiteRecue, l.QuantiteAnnulee,
                                                                a.Code AS ArticleCode, a.Designation AS ArticleDesignation, a.Marque AS ArticleMarque, a.Modele AS ArticleModele, a.UniteMesure
                                                         FROM CommandeAchatLigne l INNER JOIN Article a ON a.Id = l.IdArticle
                                                         WHERE l.Id = @Id";

        public static string Soumettre { get; } = @"UPDATE CommandeAchat SET Statut = 1, DateSoumission = GETDATE(), UserSoumission = @UserName, MotifRejet = NULL
                                                   WHERE Id = @Id AND Statut IN (0, 6) AND EXISTS (SELECT 1 FROM CommandeAchatLigne WHERE IdCommande = @Id)";

        public static string Valider { get; } = @"UPDATE CommandeAchat SET Statut = 2, DateValidation = GETDATE(), UserValidation = @UserName
                                                 WHERE Id = @Id AND Statut = 1";

        public static string Rejeter { get; } = @"UPDATE CommandeAchat SET Statut = 6, MotifRejet = @Motif, DateValidation = GETDATE(), UserValidation = @UserName
                                                 WHERE Id = @Id AND Statut = 1";

        public static string Cloturer { get; } = @"UPDATE CommandeAchat SET Statut = 5, DateCloture = GETDATE(), UserCloture = @UserName, MotifCloture = @Motif
                                                  WHERE Id = @Id AND Statut IN (2, 3)";

        public static string AnnulerReliquats { get; } = @"UPDATE CommandeAchatLigne SET QuantiteAnnulee = Quantite - QuantiteRecue
                                                          WHERE IdCommande = @Id AND Quantite > QuantiteRecue";

        /* Statut recalculé après une réception : 4 si tout est reçu, sinon 3 */
        public static string RecalculerStatut { get; } = @"UPDATE CommandeAchat
                                                          SET Statut = CASE WHEN NOT EXISTS (SELECT 1 FROM CommandeAchatLigne WHERE IdCommande = @Id AND Quantite > QuantiteRecue + QuantiteAnnulee) THEN 4 ELSE 3 END
                                                          WHERE Id = @Id AND Statut IN (2, 3)";

        public static string InsertReception { get; } = @"INSERT INTO Reception (IdCommande, DateReception, IdLocal, Bordereau, Commentaire, UserCreation)
                                                         VALUES (@IdCommande, @DateReception, @IdLocal, @Bordereau, @Commentaire, @UserCreation);
                                                         SELECT CAST(SCOPE_IDENTITY() AS bigint)";

        public static string InsertReceptionLigne { get; } = @"INSERT INTO ReceptionLigne (IdReception, IdLigneCommande, Quantite)
                                                              VALUES (@IdReception, @IdLigneCommande, @Quantite);
                                                              UPDATE CommandeAchatLigne SET QuantiteRecue = QuantiteRecue + @Quantite WHERE Id = @IdLigneCommande;
                                                              SELECT CAST(SCOPE_IDENTITY() AS bigint)";

        public static string UpdateImmoAcquisition { get; } = @"UPDATE Immo SET IdReceptionLigne = @IdReceptionLigne, PrixAcquisition = @PrixAcquisition, DateAcquisition = @DateAcquisition,
                                                                                DeviseAcquisition = @DeviseAcquisition, TauxAcquisition = @TauxAcquisition
                                                               WHERE Id = @Id";

        private const string ColonnesReception = @"r.Id, r.IdCommande, r.DateReception, r.IdLocal, r.Bordereau, r.Commentaire, r.DateCreation, r.UserCreation,
                                                   c.Numero AS NumeroCommande, f.Nom AS Fournisseur, lo.Code AS LocalCode, lo.Designation AS LocalDesignation,
                                                   ISNULL(u.Prenom + ' ', '') + ISNULL(u.Nom, r.UserCreation) AS RecuPar,
                                                   ISNULL(q.QuantiteRecue, 0) AS QuantiteRecue, ISNULL(b.NbreBiens, 0) AS NbreBiens";

        private const string JointuresReception = @"FROM Reception r
                                                    INNER JOIN CommandeAchat c ON c.Id = r.IdCommande
                                                    INNER JOIN ArticleFournisseur f ON f.Id = c.IdFournisseur
                                                    INNER JOIN Local lo ON lo.Id = r.IdLocal
                                                    LEFT JOIN _Utilisateur u ON u.UserName = r.UserCreation
                                                    LEFT JOIN (SELECT IdReception, SUM(Quantite) AS QuantiteRecue FROM ReceptionLigne GROUP BY IdReception) q ON q.IdReception = r.Id
                                                    LEFT JOIN (SELECT rl.IdReception, COUNT(i.Id) AS NbreBiens FROM ReceptionLigne rl INNER JOIN Immo i ON i.IdReceptionLigne = rl.Id GROUP BY rl.IdReception) b ON b.IdReception = r.Id";

        public static string SelectReceptions { get; } = "SELECT " + ColonnesReception + " " + JointuresReception + " ORDER BY r.DateReception DESC, r.Id DESC";

        public static string SelectReceptionsByCommande { get; } = "SELECT " + ColonnesReception + " " + JointuresReception + " WHERE r.IdCommande = @IdCommande ORDER BY r.DateReception, r.Id";

        public static string SelectReceptionById { get; } = "SELECT " + ColonnesReception + " " + JointuresReception + " WHERE r.Id = @Id";

        public static string SelectReceptionLignes { get; } = @"SELECT rl.Id, rl.IdReception, rl.IdLigneCommande, rl.Quantite,
                                                                       cl.IdArticle, a.Designation AS ArticleDesignation, cl.PrixUnitaire, r.DateReception,
                                                                       ISNULL(u.Prenom + ' ', '') + ISNULL(u.Nom, r.UserCreation) AS RecuPar,
                                                                       (SELECT COUNT(*) FROM Immo i WHERE i.IdReceptionLigne = rl.Id) AS NbreBiens
                                                                FROM ReceptionLigne rl
                                                                     INNER JOIN Reception r ON r.Id = rl.IdReception
                                                                     INNER JOIN CommandeAchatLigne cl ON cl.Id = rl.IdLigneCommande
                                                                     INNER JOIN Article a ON a.Id = cl.IdArticle
                                                                     LEFT JOIN _Utilisateur u ON u.UserName = r.UserCreation
                                                                WHERE rl.IdReception = @IdReception
                                                                ORDER BY rl.Id";

        public static string SelectImmosByReception { get; } = @"SELECT i.Id, i.CodeADM, i.CodeImmo, i.IdArticle, i.IdLocal, i.PrixAcquisition, i.DateAcquisition, i.DeviseAcquisition, i.TauxAcquisition
                                                                 FROM Immo i INNER JOIN ReceptionLigne rl ON rl.Id = i.IdReceptionLigne
                                                                 WHERE rl.IdReception = @IdReception
                                                                 ORDER BY i.Id";

        public static string InsertPiece { get; } = @"INSERT INTO CommandeAchatPiece (Id, IdCommande, NomFichier, Extension, TypePiece, UserCreation)
                                                     VALUES (@Id, @IdCommande, @NomFichier, @Extension, @TypePiece, @UserCreation)";

        public static string SelectPiecesByCommande { get; } = @"SELECT Id, IdCommande, NomFichier, Extension, TypePiece, DateCreation, UserCreation
                                                                 FROM CommandeAchatPiece WHERE IdCommande = @IdCommande ORDER BY DateCreation";

        public static string SelectPieceById { get; } = @"SELECT Id, IdCommande, NomFichier, Extension, TypePiece, DateCreation, UserCreation
                                                          FROM CommandeAchatPiece WHERE Id = @Id";

        /* Historique d'achat d'un article : une ligne par réception */
        public static string SelectAchatsByArticle { get; } = @"SELECT c.Id AS IdCommande, c.Numero AS NumeroCommande, f.Nom AS Fournisseur, r.DateReception,
                                                                       rl.Quantite, cl.PrixUnitaire, c.Devise, c.Taux,
                                                                       ISNULL(u.Prenom + ' ', '') + ISNULL(u.Nom, r.UserCreation) AS RecuPar
                                                                FROM ReceptionLigne rl
                                                                     INNER JOIN Reception r ON r.Id = rl.IdReception
                                                                     INNER JOIN CommandeAchatLigne cl ON cl.Id = rl.IdLigneCommande
                                                                     INNER JOIN CommandeAchat c ON c.Id = cl.IdCommande
                                                                     INNER JOIN ArticleFournisseur f ON f.Id = c.IdFournisseur
                                                                     LEFT JOIN _Utilisateur u ON u.UserName = r.UserCreation
                                                                WHERE cl.IdArticle = @IdArticle
                                                                ORDER BY r.DateReception DESC, r.Id DESC";

        /* Dernier prix d'achat connu d'un article */
        public static string SelectDernierPrixArticle { get; } = @"SELECT TOP 1 cl.PrixUnitaire
                                                                   FROM ReceptionLigne rl
                                                                        INNER JOIN Reception r ON r.Id = rl.IdReception
                                                                        INNER JOIN CommandeAchatLigne cl ON cl.Id = rl.IdLigneCommande
                                                                   WHERE cl.IdArticle = @IdArticle
                                                                   ORDER BY r.DateReception DESC, r.Id DESC";
    }
}
