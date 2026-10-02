/* ============================================================================
   Locate - Biens d'un organe « par article » et « en détails ». Idempotent.

   Comme pour un local (/article/local/{idLocal} et /immo/local/{idLocal}/article/{idArticle}) :
   - GET /article/organe/{codeOrgane} : biens de l'organe groupés par article ;
   - GET /immo/organe/select/{codeOrgane}/article/{idArticle} : biens d'un article dans l'organe.
   Les routes sont déclarées dans le groupe Invisible (9), sur le modèle de leurs voisines ;
   attribuez-les aux profils via Privilèges (mêmes droits que /immo/organe/select/{codeOrgane}/).
   ============================================================================ */
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @Routes TABLE (Nom nvarchar(250), Url nvarchar(200), Libelle nvarchar(200), Modele nvarchar(250));
INSERT INTO @Routes VALUES
 (N'GetArticleOrganeCodeorgane',                    N'/article/organe/{codeOrgane}',                       N'Article : biens d''un organe par article', N'GetArticleLocalIdlocal'),
 (N'GetImmoOrganeSelectCodeorganeArticleIdarticle', N'/immo/organe/select/{codeOrgane}/article/{idArticle}', N'Bien : biens d''un article dans un organe', N'GetImmoLocalIdlocalArticleIdarticle');

INSERT INTO dbo._Menu (Nom, Libelle, Icone, Commentaire, Titre, Url, Ordre, IdGroupMenu, Visible, IdModule, IdAction, DefaultMenu)
SELECT r.Nom, r.Libelle, NULL, r.Libelle, r.Libelle, r.Url,
       ROW_NUMBER() OVER (ORDER BY r.Nom) + ISNULL((SELECT MAX(x.Ordre) FROM dbo._Menu x WHERE x.IdGroupMenu = 9), 0),
       9, 0, m.IdModule, m.IdAction, 0
FROM @Routes r
     INNER JOIN dbo._Menu m ON m.Nom = r.Modele
WHERE NOT EXISTS (SELECT 1 FROM dbo._Menu x WHERE x.Nom = r.Nom);

/* Mêmes droits que la liste détaillée des biens d'un organe */
INSERT INTO dbo._Claim (IdProfil, IdMenu)
SELECT DISTINCT c.IdProfil, n.IdMenu
FROM dbo._Claim c
     INNER JOIN dbo._Menu o ON o.IdMenu = c.IdMenu AND o.Nom = N'GetImmoOrganeSelectCodeorgane'
     CROSS JOIN (SELECT IdMenu FROM dbo._Menu WHERE Nom IN (SELECT Nom FROM @Routes)) n
WHERE NOT EXISTS (SELECT 1 FROM dbo._Claim x WHERE x.IdProfil = c.IdProfil AND x.IdMenu = n.IdMenu);

COMMIT TRAN;

SELECT Nom, Url, IdGroupMenu, IdModule, IdAction FROM dbo._Menu WHERE Nom IN (N'GetArticleOrganeCodeorgane', N'GetImmoOrganeSelectCodeorganeArticleIdarticle');
PRINT 'Routes « organe par article » installées.';
