/* ============================================================================
   Locate - Déclassement et cession des biens, comme eTracking. Idempotent.

   Modèle eTracking repris à l'identique :
   - Déclasser un bien (POST /immo/declasser/{id}) : le bien passe dans le local
     « Déclassé » (IdTypeLocal = 4) de son entité ; il reste actif.
   - Liste des déclassés (GET /immo/declasser/) : contenu de ce local.
   - Céder un bien (POST /immo/cession/{id}) ou tout le local des déclassés
     (POST /immo/local/cession/) : sortie définitive (IsActive = 0, DateCession).

   Ce script :
   1. ajoute Immo.DateCession et Immo.UserCession ;
   2. ajoute le type de local « Déclassé » (Id 4) ;
   3. crée un local « Déclassé » par entité active de l'organigramme courant
      (même méthode que les scripts eTracking des locaux Non vu et Transit) ;
   4. reprend les biens déjà sortis par l'ancien déclassement de Locate
      (IsActive = 0 avec DateDeclassement) comme des biens cédés ;
   5. déclare les routes dans _Menu (groupe Invisible 9) et retire la route
      provisoire /immo/declasses/ ; attribuez-les aux profils via Privilèges.
   ============================================================================ */
SET NOCOUNT ON;
SET XACT_ABORT ON;

/* 1. Colonnes de cession */
IF COL_LENGTH('dbo.Immo', 'DateCession') IS NULL
    ALTER TABLE dbo.Immo ADD DateCession datetime NULL;
IF COL_LENGTH('dbo.Immo', 'UserCession') IS NULL
    ALTER TABLE dbo.Immo ADD UserCession nvarchar(50) NULL;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

/* 2. Type de local « Déclassé » */
IF NOT EXISTS (SELECT 1 FROM dbo.LocalType WHERE Id = 4)
BEGIN
    SET IDENTITY_INSERT dbo.LocalType ON;
    INSERT INTO dbo.LocalType (Id, Nom) VALUES (4, N'Déclassé');
    SET IDENTITY_INSERT dbo.LocalType OFF;
END

/* 3. Un local « Déclassé » par entité (code DECLASSE001, DECLASSE002…) */
DECLARE @Entites TABLE (Rang int IDENTITY(1,1), CodeOrgane nvarchar(50), Nom nvarchar(150));
INSERT INTO @Entites (CodeOrgane, Nom)
SELECT o.Id, o.Nom
FROM dbo.Organe o
WHERE o.Actif = 1 AND o.Id = o.IdStructure
  AND o.Version = (SELECT TOP 1 Id FROM dbo.OrganeVersion WHERE Defaut = 1)
  AND NOT EXISTS (SELECT 1 FROM dbo.Local l WHERE l.IdTypeLocal = 4 AND l.CodeOrgane = o.Id)
ORDER BY o.Ordre, o.OrdreInterne, o.IdStructure, o.Id;

/* Créateur des locaux : le premier compte Root (clé étrangère vers _Utilisateur) */
DECLARE @Createur nvarchar(100) = (SELECT TOP 1 u.UserName FROM dbo._Utilisateur u WHERE u.IdProfil = 1 ORDER BY u.UserName);
DECLARE @Dernier int = ISNULL((SELECT MAX(TRY_CAST(SUBSTRING(Code, 9, 10) AS int)) FROM dbo.Local WHERE IdTypeLocal = 4 AND Code LIKE 'DECLASSE%'), 0);

INSERT INTO dbo.Local (Code, Designation, CodeOrgane, IsSpace, IsActive, DateCreation, UserCreation, IdTypeLocal)
SELECT 'DECLASSE' + RIGHT('000' + CAST(@Dernier + e.Rang AS varchar(10)), CASE WHEN LEN(CAST(@Dernier + e.Rang AS varchar(10))) > 3 THEN LEN(CAST(@Dernier + e.Rang AS varchar(10))) ELSE 3 END),
       LEFT(CONCAT(N'Déclassé ', e.Nom), 150), e.CodeOrgane, 0, 1, GETDATE(), @Createur, 4
FROM @Entites e;

/* 4. Biens sortis par l'ancien déclassement de Locate : ce sont des biens cédés */
UPDATE dbo.Immo
SET DateCession = DateDeclassement, UserCession = LEFT(UserDeclassement, 50)
WHERE IsActive = 0 AND DateDeclassement IS NOT NULL AND DateCession IS NULL;

/* 5. Droits : routes eTracking déclarées dans le groupe Invisible */
DECLARE @Routes TABLE (Nom nvarchar(250), Url nvarchar(200), Libelle nvarchar(200), Modele nvarchar(250));
INSERT INTO @Routes VALUES
 (N'GetImmoDeclasser',     N'/immo/declasser/',      N'Bien : liste des déclassés',             N'GetImmoTransit'),
 (N'PostImmoDeclasserId',  N'/immo/declasser/{id}',  N'Bien : déclasser',                       N'PostImmoExpedier'),
 (N'PostImmoCessionId',    N'/immo/cession/{id}',    N'Bien : céder',                           N'PostImmoExpedier'),
 (N'PostImmoLocalCession', N'/immo/local/cession/',  N'Bien : céder tous les biens déclassés',  N'PostImmoExpedier');

INSERT INTO dbo._Menu (Nom, Libelle, Icone, Commentaire, Titre, Url, Ordre, IdGroupMenu, Visible, IdModule, IdAction, DefaultMenu)
SELECT r.Nom, r.Libelle, NULL, r.Libelle, r.Libelle, r.Url,
       ROW_NUMBER() OVER (ORDER BY r.Nom) + ISNULL((SELECT MAX(x.Ordre) FROM dbo._Menu x WHERE x.IdGroupMenu = 9), 0),
       9, 0, m.IdModule, m.IdAction, 0
FROM @Routes r
     INNER JOIN dbo._Menu m ON m.Nom = r.Modele
WHERE NOT EXISTS (SELECT 1 FROM dbo._Menu x WHERE x.Nom = r.Nom);

/* Route provisoire remplacée par GET /immo/declasser/ */
DELETE c FROM dbo._Claim c INNER JOIN dbo._Menu m ON m.IdMenu = c.IdMenu WHERE m.Nom = N'GetImmoDeclasses';
DELETE FROM dbo._Menu WHERE Nom = N'GetImmoDeclasses';

COMMIT TRAN;

SELECT (SELECT COUNT(*) FROM dbo.Local WHERE IdTypeLocal = 4) AS LocauxDeclasses,
       (SELECT COUNT(*) FROM dbo.Immo WHERE DateCession IS NOT NULL) AS BiensCedes;
PRINT 'Déclassement et cession (modèle eTracking) installés.';
