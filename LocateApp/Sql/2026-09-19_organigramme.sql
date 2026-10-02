/* ============================================================================
   Locate - Gestion graphique de l'organigramme et des versions
   Idempotent. À exécuter après 2026-09-19_menus_routes.sql.
   ============================================================================ */
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

/* Informations descriptives sur une version d'organigramme */
IF COL_LENGTH('dbo.OrganeVersion', 'Libelle') IS NULL
    ALTER TABLE dbo.OrganeVersion ADD Libelle nvarchar(100) NULL, Commentaire nvarchar(500) NULL, DateCreation datetime NULL, UserCreation nvarchar(50) NULL;

/* Le menu Versions d'organigramme devient visible dans Paramètres */
DECLARE @IdParametres int = (SELECT IdGroupMenu FROM dbo._GroupMenu WHERE Nom = 'Parametres');
UPDATE dbo._Menu
SET IdGroupMenu = @IdParametres, Visible = 1,
    Ordre = (SELECT ISNULL(MAX(Ordre), 0) + 1 FROM dbo._Menu x WHERE x.IdGroupMenu = @IdParametres),
    Libelle = 'Versions d''organigramme', Titre = 'Versions de l''organigramme',
    Commentaire = 'Création, copie et bascule de la version courante de l''organigramme'
WHERE Nom = 'GetOrganeVersion' AND (IdGroupMenu <> @IdParametres OR Visible = 0);

UPDATE dbo._Menu SET Libelle = 'Organigramme', Titre = 'Organigramme', Commentaire = 'Arborescence des organes : ajout, modification, suppression, réorganisation et versions'
WHERE Nom = 'GetOrganeConfigOrganigramme';

COMMIT TRAN;
PRINT 'Organigramme : script appliqué.';
