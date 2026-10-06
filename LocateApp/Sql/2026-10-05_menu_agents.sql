/* ============================================================================
   Locate - Répertoire des agents (Paramètres > Agents, GET /agent/), ajout et modification. Idempotent.
   Accordé à l'administrateur des inventaires (IdProfil 10) ; le root voit tout.
   ============================================================================ */
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

IF NOT EXISTS (SELECT 1 FROM dbo._Module WHERE Nom = N'Agent')
    INSERT INTO dbo._Module (Nom) VALUES (N'Agent');

DECLARE @IdModule int = (SELECT TOP 1 IdModule FROM dbo._Module WHERE Nom = N'Agent');

IF NOT EXISTS (SELECT 1 FROM dbo._Menu WHERE Nom = N'GetAgent')
    INSERT INTO dbo._Menu (Nom, Libelle, Icone, Commentaire, Titre, Url, Ordre, IdGroupMenu, Visible, IdModule, IdAction, DefaultMenu)
    VALUES (N'GetAgent', N'Agents', NULL, N'Répertoire des agents : organe, contact, compte Locate et biens dont ils sont responsables',
            N'Agents', N'/agent/', 7, 7, 1, @IdModule, 1, 0);

/* Formulaires (groupe Invisible) */
DECLARE @Routes TABLE (Nom nvarchar(250), Url nvarchar(200), Libelle nvarchar(200), IdAction int);
INSERT INTO @Routes VALUES
 (N'GetAgentAdd',             N'/agent/add/',             N'Agent : formulaire d''ajout',        2),
 (N'GetAgentModifyMatricule', N'/agent/modify/{matricule}', N'Agent : formulaire de modification', 3),
 (N'PostAgentAdd',            N'/agent/add/',             N'Agent : ajout',                      2),
 (N'PostAgentModify',         N'/agent/modify/',          N'Agent : modification',               3);
INSERT INTO dbo._Menu (Nom, Libelle, Icone, Commentaire, Titre, Url, Ordre, IdGroupMenu, Visible, IdModule, IdAction, DefaultMenu)
SELECT r.Nom, r.Libelle, NULL, r.Libelle, r.Libelle, r.Url,
       ROW_NUMBER() OVER (ORDER BY r.Nom) + ISNULL((SELECT MAX(x.Ordre) FROM dbo._Menu x WHERE x.IdGroupMenu = 9), 0),
       9, 0, @IdModule, r.IdAction, 0
FROM @Routes r
WHERE NOT EXISTS (SELECT 1 FROM dbo._Menu x WHERE x.Nom = r.Nom);

INSERT INTO dbo._Claim (IdProfil, IdMenu)
SELECT p.IdProfil, m.IdMenu
FROM dbo._Profil p CROSS JOIN dbo._Menu m
WHERE p.IdProfil = 10 AND (m.Nom = N'GetAgent' OR m.Nom IN (SELECT Nom FROM @Routes))
  AND NOT EXISTS (SELECT 1 FROM dbo._Claim c WHERE c.IdProfil = p.IdProfil AND c.IdMenu = m.IdMenu);

COMMIT TRAN;
PRINT 'Menu « Agents » et formulaires installés.';
