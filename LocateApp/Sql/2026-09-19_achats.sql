/* ============================================================================
   Locate - Module Achats (commandes fournisseurs, réceptions, création des biens)
   Script idempotent : peut être rejoué sans dommage.
   À exécuter sur la base Locate de chaque installation.
   ============================================================================ */
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

/* ---------------------------------------------------------------------------
   1. Tables
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.CommandeAchat') IS NULL
BEGIN
    CREATE TABLE dbo.CommandeAchat (
        Id              bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_CommandeAchat PRIMARY KEY,
        Numero          nvarchar(20)  NOT NULL CONSTRAINT UQ_CommandeAchat_Numero UNIQUE,
        IdFournisseur   bigint        NOT NULL CONSTRAINT FK_CommandeAchat_Fournisseur REFERENCES dbo.ArticleFournisseur(Id),
        Devise          char(3)       NOT NULL,
        /* 0 Brouillon, 1 Soumise, 2 Validée, 3 Partiellement livrée, 4 Livrée, 5 Clôturée (reliquat annulé), 6 Rejetée */
        Statut          int           NOT NULL CONSTRAINT DF_CommandeAchat_Statut DEFAULT 0,
        DateCommande    date          NOT NULL CONSTRAINT DF_CommandeAchat_DateCommande DEFAULT CAST(GETDATE() AS date),
        Commentaire     nvarchar(500) NULL,
        DateCreation    datetime      NOT NULL CONSTRAINT DF_CommandeAchat_DateCreation DEFAULT GETDATE(),
        UserCreation    nvarchar(50)  NOT NULL,
        DateSoumission  datetime      NULL,
        UserSoumission  nvarchar(50)  NULL,
        DateValidation  datetime      NULL,
        UserValidation  nvarchar(50)  NULL,
        MotifRejet      nvarchar(1000) NULL,
        DateCloture     datetime      NULL,
        UserCloture     nvarchar(50)  NULL,
        MotifCloture    nvarchar(1000) NULL
    );
    CREATE INDEX IX_CommandeAchat_Statut ON dbo.CommandeAchat (Statut) INCLUDE (IdFournisseur, DateCommande);
END;

IF OBJECT_ID('dbo.CommandeAchatLigne') IS NULL
BEGIN
    CREATE TABLE dbo.CommandeAchatLigne (
        Id              bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_CommandeAchatLigne PRIMARY KEY,
        IdCommande      bigint        NOT NULL CONSTRAINT FK_CommandeAchatLigne_Commande REFERENCES dbo.CommandeAchat(Id),
        IdArticle       bigint        NOT NULL CONSTRAINT FK_CommandeAchatLigne_Article REFERENCES dbo.Article(Id),
        Quantite        int           NOT NULL CONSTRAINT CK_CommandeAchatLigne_Quantite CHECK (Quantite > 0),
        PrixUnitaire    decimal(18,2) NOT NULL CONSTRAINT CK_CommandeAchatLigne_Prix CHECK (PrixUnitaire >= 0),
        QuantiteRecue   int           NOT NULL CONSTRAINT DF_CommandeAchatLigne_QteRecue DEFAULT 0,
        QuantiteAnnulee int           NOT NULL CONSTRAINT DF_CommandeAchatLigne_QteAnnulee DEFAULT 0
    );
    CREATE INDEX IX_CommandeAchatLigne_Commande ON dbo.CommandeAchatLigne (IdCommande);
    CREATE INDEX IX_CommandeAchatLigne_Article ON dbo.CommandeAchatLigne (IdArticle);
END;

IF OBJECT_ID('dbo.Reception') IS NULL
BEGIN
    CREATE TABLE dbo.Reception (
        Id              bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_Reception PRIMARY KEY,
        IdCommande      bigint        NOT NULL CONSTRAINT FK_Reception_Commande REFERENCES dbo.CommandeAchat(Id),
        DateReception   date          NOT NULL,
        IdLocal         bigint        NOT NULL CONSTRAINT FK_Reception_Local REFERENCES dbo.Local(Id),
        Bordereau       nvarchar(50)  NULL,
        Commentaire     nvarchar(500) NULL,
        DateCreation    datetime      NOT NULL CONSTRAINT DF_Reception_DateCreation DEFAULT GETDATE(),
        UserCreation    nvarchar(50)  NOT NULL
    );
    CREATE INDEX IX_Reception_Commande ON dbo.Reception (IdCommande);
END;

IF OBJECT_ID('dbo.ReceptionLigne') IS NULL
BEGIN
    CREATE TABLE dbo.ReceptionLigne (
        Id              bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_ReceptionLigne PRIMARY KEY,
        IdReception     bigint NOT NULL CONSTRAINT FK_ReceptionLigne_Reception REFERENCES dbo.Reception(Id),
        IdLigneCommande bigint NOT NULL CONSTRAINT FK_ReceptionLigne_LigneCommande REFERENCES dbo.CommandeAchatLigne(Id),
        Quantite        int    NOT NULL CONSTRAINT CK_ReceptionLigne_Quantite CHECK (Quantite > 0)
    );
    CREATE INDEX IX_ReceptionLigne_Reception ON dbo.ReceptionLigne (IdReception);
END;

IF OBJECT_ID('dbo.CommandeAchatPiece') IS NULL
BEGIN
    CREATE TABLE dbo.CommandeAchatPiece (
        Id              uniqueidentifier NOT NULL CONSTRAINT PK_CommandeAchatPiece PRIMARY KEY CONSTRAINT DF_CommandeAchatPiece_Id DEFAULT NEWID(),
        IdCommande      bigint        NOT NULL CONSTRAINT FK_CommandeAchatPiece_Commande REFERENCES dbo.CommandeAchat(Id),
        NomFichier      nvarchar(255) NOT NULL,
        Extension       nvarchar(10)  NOT NULL,
        TypePiece       nvarchar(20)  NOT NULL,   /* Cloture */
        DateCreation    datetime      NOT NULL CONSTRAINT DF_CommandeAchatPiece_DateCreation DEFAULT GETDATE(),
        UserCreation    nvarchar(50)  NOT NULL
    );
END;

IF COL_LENGTH('dbo.Immo', 'IdReceptionLigne') IS NULL
BEGIN
    ALTER TABLE dbo.Immo ADD
        IdReceptionLigne bigint NULL CONSTRAINT FK_Immo_ReceptionLigne REFERENCES dbo.ReceptionLigne(Id),
        PrixAcquisition  decimal(18,2) NULL,
        DateAcquisition  date NULL;
END;

/* ---------------------------------------------------------------------------
   2. Module, menus et droits
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo._Module WHERE IdModule = 10021)
BEGIN
    SET IDENTITY_INSERT dbo._Module ON;
    INSERT INTO dbo._Module (IdModule, Nom) VALUES (10021, 'Achat');
    SET IDENTITY_INSERT dbo._Module OFF;
END;

DECLARE @IdModule bigint = 10021;
DECLARE @IdGroupe int;
DECLARE @IdInvisible int = 9;   /* groupe "Invisible" : routes protégées sans entrée de menu */

IF NOT EXISTS (SELECT 1 FROM dbo._GroupMenu WHERE Nom = 'Achats')
BEGIN
    /* Le groupe Achats prend la 3e place, juste après Biens */
    UPDATE dbo._GroupMenu SET Ordre = Ordre + 1 WHERE IdSection = 1 AND Ordre >= 3;
    INSERT INTO dbo._GroupMenu (Nom, Libelle, Url, Icone, Ordre, Visible, IdSection, DefaultInternalUser, DefaultExternalUser)
    VALUES ('Achats', 'Achats', '#', 'shopping_cart', 3, 1, 1, 0, 0);
END;
SELECT @IdGroupe = IdGroupMenu FROM dbo._GroupMenu WHERE Nom = 'Achats';

/* Menus : (Nom = claim calculé à partir de la route, Libelle, Titre, Commentaire, Url, Ordre, Groupe, Visible, Action) */
DECLARE @Menus TABLE (Nom nvarchar(250), Libelle nvarchar(200), Titre nvarchar(200), Commentaire nvarchar(400), Url nvarchar(200), Ordre int, IdGroupMenu int, Visible bit, IdAction int);
INSERT INTO @Menus VALUES
 ('GetAchatCommande',              'Commandes',            'Commandes d''achat',          'Suivi des commandes fournisseurs',                          '/achat/commande/',              1, @IdGroupe,    1, 1),
 ('GetAchatCommandeAdd',           'Nouvelle commande',    'Nouvelle commande d''achat',  'Saisie d''une commande fournisseur',                        '/achat/commande/add/',          2, @IdGroupe,    1, 2),
 ('GetAchatCommandeAvalider',      'À valider',            'Commandes à valider',         'Commandes soumises en attente de validation',               '/achat/commande/avalider/',     3, @IdGroupe,    1, 1),
 ('GetAchatReception',             'Réceptions',           'Réceptions',                  'Historique des livraisons reçues',                          '/achat/reception/',             4, @IdGroupe,    1, 1),
 ('GetAchatCommandeIdId',          'Commande',             'Commande d''achat',           'Détail d''une commande fournisseur',                        '/achat/commande/id/{id}',       1, @IdInvisible, 0, 1),
 ('GetAchatCommandeModifyId',      'Modifier la commande', 'Modifier la commande',        'Modification d''une commande en brouillon',                 '/achat/commande/modify/{id}',   2, @IdInvisible, 0, 3),
 ('PostAchatCommandeAdd',          'Créer la commande',    'Créer la commande',           'Enregistrement d''une commande',                            '/achat/commande/add/',          3, @IdInvisible, 0, 2),
 ('PostAchatCommandeModify',       'Modifier la commande', 'Modifier la commande',        'Enregistrement des modifications',                          '/achat/commande/modify/',       4, @IdInvisible, 0, 3),
 ('GetAchatCommandeSoumettreId',   'Soumettre',            'Soumettre la commande',       'Envoi d''une commande en validation',                       '/achat/commande/soumettre/{id}',5, @IdInvisible, 0, 3),
 ('GetAchatCommandeValiderId',     'Valider',              'Valider la commande',         'Validation de second niveau',                               '/achat/commande/valider/{id}',  6, @IdInvisible, 0, 3),
 ('PostAchatCommandeRejeter',      'Rejeter',              'Rejeter la commande',         'Renvoi d''une commande au demandeur avec motif',             '/achat/commande/rejeter/',      7, @IdInvisible, 0, 3),
 ('PostAchatCommandeCloturer',     'Clôturer',             'Clôturer la commande',        'Annulation du reliquat avec pièces justificatives',         '/achat/commande/cloturer/',     8, @IdInvisible, 0, 3),
 ('GetAchatCommandeSupprimerId',   'Supprimer',            'Supprimer la commande',       'Suppression d''un brouillon',                               '/achat/commande/supprimer/{id}',9, @IdInvisible, 0, 4),
 ('GetAchatCommandePieceId',       'Pièce jointe',         'Pièce jointe',                'Téléchargement d''une pièce justificative',                 '/achat/commande/piece/{id}',   10, @IdInvisible, 0, 1),
 ('GetAchatReceptionAddIdcommande','Réceptionner',         'Réception de livraison',      'Enregistrement d''une livraison totale ou partielle',       '/achat/reception/add/{idCommande}', 11, @IdInvisible, 0, 2),
 ('PostAchatReceptionAdd',         'Réceptionner',         'Réception de livraison',      'Validation d''une réception : création des biens',          '/achat/reception/add/',        12, @IdInvisible, 0, 2),
 ('GetAchatReceptionIdId',         'Réception',            'Réception',                   'Détail d''une livraison',                                   '/achat/reception/id/{id}',     13, @IdInvisible, 0, 1);

/* L'ordre est unique par groupe : on place les nouveaux menus après les existants */
INSERT INTO dbo._Menu (Nom, Libelle, Icone, Commentaire, Titre, Url, Ordre, IdGroupMenu, Visible, IdModule, IdAction, DefaultMenu)
SELECT m.Nom, m.Libelle, NULL, m.Commentaire, m.Titre, m.Url,
       m.Ordre + ISNULL((SELECT MAX(x.Ordre) FROM dbo._Menu x WHERE x.IdGroupMenu = m.IdGroupMenu), 0),
       m.IdGroupMenu, m.Visible, @IdModule, m.IdAction, 0
FROM @Menus m
WHERE NOT EXISTS (SELECT 1 FROM dbo._Menu x WHERE x.Nom = m.Nom);

/* Profils métier : Acheteur (saisie + réception) et Validateur des achats (second niveau, clôture) */
IF NOT EXISTS (SELECT 1 FROM dbo._Profil WHERE Nom = 'Acheteur')
    INSERT INTO dbo._Profil (IdProfil, Nom, Libelle, Root, DefaultProfil, Externe, HomeUrl)
    VALUES ((SELECT MAX(IdProfil) + 1 FROM dbo._Profil), 'Acheteur', 'Acheteur', 0, 0, 0, '/achat/commande/');
IF NOT EXISTS (SELECT 1 FROM dbo._Profil WHERE Nom = 'ValidateurAchat')
    INSERT INTO dbo._Profil (IdProfil, Nom, Libelle, Root, DefaultProfil, Externe, HomeUrl)
    VALUES ((SELECT MAX(IdProfil) + 1 FROM dbo._Profil), 'ValidateurAchat', 'Validateur des achats', 0, 0, 0, '/achat/commande/avalider/');

DECLARE @Acheteur int = (SELECT IdProfil FROM dbo._Profil WHERE Nom = 'Acheteur');
DECLARE @Validateur int = (SELECT IdProfil FROM dbo._Profil WHERE Nom = 'ValidateurAchat');

DECLARE @Droits TABLE (IdProfil int, Nom nvarchar(250));
INSERT INTO @Droits
SELECT @Acheteur, n FROM (VALUES ('Get'), ('GetAchatCommande'), ('GetAchatCommandeAdd'), ('PostAchatCommandeAdd'), ('GetAchatCommandeIdId'),
                                  ('GetAchatCommandeModifyId'), ('PostAchatCommandeModify'), ('GetAchatCommandeSoumettreId'), ('GetAchatCommandeSupprimerId'),
                                  ('GetAchatCommandePieceId'), ('GetAchatReception'), ('GetAchatReceptionAddIdcommande'), ('PostAchatReceptionAdd'), ('GetAchatReceptionIdId'),
                                  ('GetArticle'), ('GetFournisseur'), ('GetArticleIdId')) v(n)
UNION ALL
SELECT @Validateur, n FROM (VALUES ('Get'), ('GetAchatCommande'), ('GetAchatCommandeAvalider'), ('GetAchatCommandeIdId'), ('GetAchatCommandeValiderId'),
                                    ('PostAchatCommandeRejeter'), ('PostAchatCommandeCloturer'), ('GetAchatCommandePieceId'),
                                    ('GetAchatReception'), ('GetAchatReceptionIdId'), ('GetArticleIdId')) v(n);

INSERT INTO dbo._Claim (IdProfil, IdMenu)
SELECT d.IdProfil, m.IdMenu
FROM @Droits d JOIN dbo._Menu m ON m.Nom = d.Nom
WHERE NOT EXISTS (SELECT 1 FROM dbo._Claim c WHERE c.IdProfil = d.IdProfil AND c.IdMenu = m.IdMenu);

/* ---------------------------------------------------------------------------
   3. Nettoyage des menus et profils hérités d'un ancien projet
   --------------------------------------------------------------------------- */
/* Menus de reporting "passage patient" */
DELETE FROM dbo._Claim WHERE IdMenu IN (SELECT IdMenu FROM dbo._Menu WHERE Nom IN ('GetReportingPassageIdHopital', 'GetReportingPassagePatient{IdInstitution_}Matricule', 'GetProfileModifyMatricule'));
DELETE FROM dbo._Menu  WHERE Nom IN ('GetReportingPassageIdHopital', 'GetReportingPassagePatient{IdInstitution_}Matricule', 'GetProfileModifyMatricule');

/* Les routes /famille/details/ servent aux graphiques du tableau de bord : on les garde, mais dans le groupe Invisible */
UPDATE m SET IdGroupMenu = @IdInvisible, Visible = 0, Ordre = (SELECT MAX(x.Ordre) FROM dbo._Menu x WHERE x.IdGroupMenu = @IdInvisible) + 1
FROM dbo._Menu m WHERE m.Nom = 'GetFamilleDetails' AND m.IdGroupMenu <> @IdInvisible;
UPDATE m SET IdGroupMenu = @IdInvisible, Visible = 0, Ordre = (SELECT MAX(x.Ordre) FROM dbo._Menu x WHERE x.IdGroupMenu = @IdInvisible) + 1
FROM dbo._Menu m WHERE m.Nom = 'GetFamilleDetailsEtat' AND m.IdGroupMenu <> @IdInvisible;

/* Groupes sans objet : "Rechercher un Ayant droit" et "Factures" */
DELETE FROM dbo._GroupMenu WHERE Nom IN ('Recherche', 'Factures') AND NOT EXISTS (SELECT 1 FROM dbo._Menu m WHERE m.IdGroupMenu = dbo._GroupMenu.IdGroupMenu);

/* Profils hospitaliers, plus aucun utilisateur */
DELETE FROM dbo._Claim WHERE IdProfil IN (SELECT IdProfil FROM dbo._Profil WHERE Nom IN ('DRHCentreSante', 'AcceuilHopital', 'FacturationHopital', 'DRHAdminMedical'))
  AND NOT EXISTS (SELECT 1 FROM dbo._Utilisateur u WHERE u.IdProfil = dbo._Claim.IdProfil);
DELETE FROM dbo._Profil WHERE Nom IN ('DRHCentreSante', 'AcceuilHopital', 'FacturationHopital', 'DRHAdminMedical')
  AND NOT EXISTS (SELECT 1 FROM dbo._Utilisateur u WHERE u.IdProfil = dbo._Profil.IdProfil);

/* Libellés génériques et corrections */
UPDATE dbo._Profil SET Libelle = 'Agent' WHERE Nom = 'Agent' AND Libelle LIKE 'Agent %';
UPDATE dbo._Menu SET Libelle = 'Ajouter un article' WHERE Libelle = 'Ajouter Ariticle';
UPDATE dbo._Menu SET Url = '/profil/modify/{idProfil}' WHERE Nom = 'GetProfilModifyIdProfil' AND Url = '/profil/modify/{idProfil';
UPDATE dbo._Menu SET Libelle = 'Privilège' WHERE Libelle = 'Privillège';

COMMIT TRAN;
PRINT 'Module Achats installé.';
