/* =====================================================================================================
   Locate - Profils et droits par défaut. Idempotent (les droits sont désignés par le nom du menu, pas par son identifiant).
   - Super administrateur (IdProfil 1, Root) : tous les droits, sans liste.
   - Agent de terrain (IdProfil 2) : consultation, opérations de terrain et routes de l'application mobile.
   - Administrateur des inventaires (IdProfil 10) : tout sauf la sécurité (profils, droits, menus, utilisateurs, journal).
   - Routes publiques (erreurs, connexion) déclarées dans _Menu ; route /profile/modify/{matricule} retirée.
   ===================================================================================================== */
SET XACT_ABORT ON;
SET NOCOUNT ON;
BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM dbo._Profil WHERE IdProfil = 10)
    INSERT INTO dbo._Profil (IdProfil, Nom, Libelle, Root, DefaultProfil, Externe, HomeUrl)
    VALUES (10, N'Administrateur', N'Administrateur des inventaires', 0, 0, 0, N'/');
UPDATE dbo._Profil SET Libelle = N'Super administrateur' WHERE IdProfil = 1;
UPDATE dbo._Profil SET Libelle = N'Agent de terrain', HomeUrl = N'/' WHERE IdProfil = 2;

/* Route retirée du code */
DELETE c FROM dbo._Claim c JOIN dbo._Menu m ON m.IdMenu = c.IdMenu WHERE m.Nom = N'GetProfileModifyMatricule';
DELETE FROM dbo._Menu WHERE Nom = N'GetProfileModifyMatricule';

/* Routes publiques */
DECLARE @Publiques TABLE (Nom nvarchar(100), Libelle nvarchar(100), Url nvarchar(200), IdModule int);
INSERT INTO @Publiques VALUES
    (N'GetError', N'Erreur', N'/error/', 3), (N'GetErrorCode', N'Erreur : page d''un code d''erreur', N'/error/{code}', 3),
    (N'GetAuthLogin', N'Connexion : formulaire', N'/auth/login', 4), (N'GetAuthLogout', N'Déconnexion', N'/auth/logout', 4),
    (N'PostAuthLogin', N'Connexion : validation', N'/auth/login', 4);
DECLARE @Ordre int = (SELECT ISNULL(MAX(Ordre), 0) FROM dbo._Menu);
INSERT INTO dbo._Menu (Nom, Libelle, Icone, Commentaire, Titre, Url, Ordre, IdGroupMenu, Visible, IdModule, IdAction, DefaultMenu)
SELECT p.Nom, p.Libelle, NULL, p.Libelle + N' (route publique)', p.Libelle, p.Url, @Ordre + ROW_NUMBER() OVER (ORDER BY p.Nom), 9, 0, p.IdModule, 1, 0
FROM @Publiques p WHERE NOT EXISTS (SELECT 1 FROM dbo._Menu m WHERE m.Nom = p.Nom);

/* Agent de terrain */
DECLARE @Terrain TABLE (Nom nvarchar(100) PRIMARY KEY);
INSERT INTO @Terrain VALUES
    (N'Get'), (N'GetArticle'), (N'GetArticleIdId'), (N'GetArticleLocalIdlocal'), (N'GetArticleEntiteCodeorgane'), (N'GetArticleOrganeCodeorgane'), (N'GetImmoEntiteSelectCodeorganeArticleIdarticle'), (N'GetArticleSelectDesignation'),
    (N'GetCategorieIdId'), (N'GetCategorieSelectNom'), (N'GetEspace'), (N'GetEspaceIdId'), (N'GetEspaceOrgane'), (N'GetEspaceOrganeCodeorgane'),
    (N'GetEspaceQrcodeQrcode'), (N'GetEspaceSelectDesignation'), (N'GetFamilleDetails'), (N'GetFamilleDetailsEtat'), (N'GetFamilleIdentifie'),
    (N'GetFamilleIdId'), (N'GetFamilleSelectNom'), (N'GetFamilleStatPie'), (N'GetFamilleStatPieLegend'), (N'GetFamilleStatRadar'), (N'GetImmo'),
    (N'GetImmoAddIdlocal'), (N'GetImmoArticleIdarticle'), (N'GetImmoDeclassementIdimmo'), (N'GetImmoDeclasser'), (N'GetImmoEntiteList'),
    (N'GetImmoEntiteList{IdInstitution_}'), (N'GetImmoEntiteSelect'), (N'GetImmoEntiteSelectCodeorgane'), (N'GetImmoEtiquetteIdetiquette'),
    (N'GetImmoIdId'), (N'GetImmoLiveqrcodeIdimmo'), (N'GetImmoLocalIdlocal'), (N'GetImmoLocalIdlocalArticleIdarticle'), (N'GetImmoModifyId'),
    (N'GetImmoNonvu'), (N'GetImmoOrganeList'), (N'GetImmoOrganeList{IdInstitution_}'), (N'GetImmoOrganeSelectCodeorgane'),
    (N'GetImmoOrganeSelectCodeorganeArticleIdarticle'), (N'GetImmoPrincipalIdlocal'), (N'GetImmoQrcodeQrcode'), (N'GetImmoResetqrcodeIdimmo'),
    (N'GetImmoResponsableBienlitigieuxMatricule'), (N'GetImmoResponsableBienMatricule'), (N'GetImmoResponsableList'),
    (N'GetImmoResponsableList{IdInstitution_}'), (N'GetImmoResponsableLitigieux'), (N'GetImmoResponsableOrganeCodeorgane'), (N'GetImmoSanslocal'),
    (N'GetImmoSelect'), (N'GetImmoSelectCode'), (N'GetImmoTransit'), (N'GetInventaire'), (N'GetInventaireDetailsAnnee'), (N'GetInventaireEncours'),
    (N'GetInventaireOrganeCodeorgane'), (N'GetInventaireOrganeList{IdInstitution_}'), (N'GetInventaireResultat'), (N'GetInventaireResultatAnnee'),
    (N'GetInventaireSelectAnnee'), (N'GetLocal'), (N'GetLocalAddCodeorgane'), (N'GetLocalEtiquetteIdetiquette'), (N'GetLocalIdId'),
    (N'GetLocalLiveqrcodeIdlocal'), (N'GetLocalModifyId'), (N'GetLocalOrganeCodeorgane'), (N'GetLocalOrganeList'),
    (N'GetLocalOrganeList{IdInstitution_}'), (N'GetLocalQrcodeQrcode'), (N'GetLocalResetqrcodeIdlocal'), (N'GetLocalSelectDesignation'),
    (N'GetObservation'), (N'GetObservationEtatEtat'), (N'GetObservationSelectIdobservation'), (N'GetObservationStat'), (N'GetOrgane'),
    (N'GetOrganeAutonome'), (N'GetOrganeEntite'), (N'GetOrganeEntiteCode'), (N'GetOrganeOrganigramme'), (N'GetOrganeOrganigrammeId'),
    (N'GetOrganeOrganigrammeVersionId'), (N'GetOrganeSelect'), (N'GetOrganeSelectCode'), (N'GetProfile'), (N'PostImmoAdd'), (N'PostImmoAddPhoto'),
    (N'PostImmoCessionId'), (N'PostImmoChangelocal'), (N'PostImmoDeclasserId'), (N'PostImmoExpedier'), (N'PostImmoLocalCession'),
    (N'PostImmoMisenservice'), (N'PostImmoModify'), (N'PostImmoNonvuId'), (N'PostImmoQrcode'), (N'PostImmoSelect'), (N'PostImmoSyncAdd'),
    (N'PostImmoSyncModify'), (N'PostInventaireDetailsAdd'), (N'PostInventaireDetailsLocal'), (N'PostInventaireIdentifier'),
    (N'PostInventaireNonvuAdd'), (N'PostLocalAdd'), (N'PostLocalModify'), (N'PostLocalQrcode'), (N'PostLocalSyncAdd'), (N'PostLocalSyncQrcode');
DELETE FROM dbo._Claim WHERE IdProfil = 2;
INSERT INTO dbo._Claim (IdProfil, IdMenu) SELECT 2, m.IdMenu FROM dbo._Menu m JOIN @Terrain t ON t.Nom = m.Nom;

/* Administrateur des inventaires : tous les menus sauf la sécurité */
DELETE FROM dbo._Claim WHERE IdProfil = 10;
INSERT INTO dbo._Claim (IdProfil, IdMenu)
SELECT 10, m.IdMenu FROM dbo._Menu m
WHERE m.Nom NOT LIKE N'GetClaim%' AND m.Nom NOT LIKE N'PostClaim%'
  AND NOT ((m.Nom LIKE N'GetProfil%' OR m.Nom LIKE N'PostProfil%') AND m.Nom NOT LIKE N'GetProfile%')
  AND m.Nom NOT LIKE N'GetMenu%' AND m.Nom NOT LIKE N'PostMenu%' AND m.Nom NOT LIKE N'GetGroupmenu%' AND m.Nom NOT LIKE N'PostGroupmenu%'
  AND m.Nom NOT LIKE N'GetUtilisateur%' AND m.Nom NOT LIKE N'PostUtilisateur%' AND m.Nom <> N'GetReportingLog'
  AND m.Nom NOT IN (SELECT Nom FROM @Publiques);

COMMIT TRANSACTION;
PRINT 'Profils et droits par défaut installés.';
