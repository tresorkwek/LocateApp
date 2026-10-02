/* ============================================================================
   Locate - Enregistrement de toutes les routes comme menus (droits par profil)
   Généré automatiquement le 2026-09-19 à partir des modules Nancy. Idempotent.
   ============================================================================ */
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;


/* Corrections de casse : le nom doit être exactement celui calculé par GetClaimString */
UPDATE dbo._Menu SET Nom = N'GetMenuModifyIdmenu', Url = N'/menu/modify/{idMenu}' WHERE IdMenu = 81 AND Nom COLLATE Latin1_General_CS_AS = N'GetMenuModifyIdMenu' AND NOT EXISTS (SELECT 1 FROM dbo._Menu WHERE Nom = N'GetMenuModifyIdmenu' AND IdMenu <> 81);
UPDATE dbo._Menu SET Nom = N'GetProfilSelectIdprofil', Url = N'/profil/select/{idProfil}' WHERE IdMenu = 2127 AND Nom COLLATE Latin1_General_CS_AS = N'GetProfilSelectIdProfil' AND NOT EXISTS (SELECT 1 FROM dbo._Menu WHERE Nom = N'GetProfilSelectIdprofil' AND IdMenu <> 2127);

/* Routes absentes : ajoutées dans le groupe Invisible, à attribuer aux profils via l'écran Privilèges */
DECLARE @Routes TABLE (Nom nvarchar(250), Module nvarchar(100), IdAction int, Url nvarchar(200), Libelle nvarchar(200));
INSERT INTO @Routes VALUES
 (N'PostOrganeOrganigrammeReorder', N'Organe', 3, N'/organe/organigramme/reorder', N'Organe : organigramme reorder (enregistrement)'),
 (N'GetOrganeDeleteId', N'Organe', 4, N'/organe/delete/{id}', N'Organe : suppression'),
 (N'GetOrganeVersionDeleteId', N'OrganeVersion', 4, N'/organe/version/delete/{id}', N'Version d''organigramme : suppression');

INSERT INTO dbo._Menu (Nom, Libelle, Icone, Commentaire, Titre, Url, Ordre, IdGroupMenu, Visible, IdModule, IdAction, DefaultMenu)
SELECT r.Nom, r.Libelle, NULL, r.Libelle, r.Libelle, r.Url,
       ROW_NUMBER() OVER (ORDER BY r.Nom) + ISNULL((SELECT MAX(x.Ordre) FROM dbo._Menu x WHERE x.IdGroupMenu = 9), 0),
       9, 0, m.IdModule, r.IdAction, 0
FROM @Routes r
     INNER JOIN dbo._Module m ON RTRIM(m.Nom) = r.Module
WHERE NOT EXISTS (SELECT 1 FROM dbo._Menu x WHERE x.Nom = r.Nom);

COMMIT TRAN;
PRINT 'Routes enregistrées.';
