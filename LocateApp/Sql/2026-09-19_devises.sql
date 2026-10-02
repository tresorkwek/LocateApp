/* ============================================================================
   Locate - Table des devises et taux de conversion
   Montant en devise de référence = Montant x Taux (USD : taux 1).
   Script idempotent, à exécuter après 2026-09-19_achats.sql.
   ============================================================================ */
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

IF OBJECT_ID('dbo.Devise') IS NULL
BEGIN
    CREATE TABLE dbo.Devise (
        Code         char(3)       NOT NULL CONSTRAINT PK_Devise PRIMARY KEY,
        Libelle      nvarchar(50)  NOT NULL,
        Symbole      nvarchar(5)   NULL,
        Taux         decimal(18,6) NOT NULL CONSTRAINT CK_Devise_Taux CHECK (Taux > 0),
        EstReference bit           NOT NULL CONSTRAINT DF_Devise_EstReference DEFAULT 0,
        Actif        bit           NOT NULL CONSTRAINT DF_Devise_Actif DEFAULT 1,
        DateMaj      datetime      NOT NULL CONSTRAINT DF_Devise_DateMaj DEFAULT GETDATE(),
        UserMaj      nvarchar(50)  NULL
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Devise)
    INSERT INTO dbo.Devise (Code, Libelle, Symbole, Taux, EstReference, Actif) VALUES ('USD', 'Dollar américain', '$', 1, 1, 1);

/* Taux figé sur la commande au moment de sa saisie */
IF COL_LENGTH('dbo.CommandeAchat', 'Taux') IS NULL
    ALTER TABLE dbo.CommandeAchat ADD Taux decimal(18,6) NOT NULL CONSTRAINT DF_CommandeAchat_Taux DEFAULT 1;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CommandeAchat_Devise')
BEGIN
    /* les commandes déjà saisies pointent vers une devise existante */
    INSERT INTO dbo.Devise (Code, Libelle, Taux, EstReference, Actif)
    SELECT DISTINCT c.Devise, c.Devise, 1, 0, 1 FROM dbo.CommandeAchat c WHERE NOT EXISTS (SELECT 1 FROM dbo.Devise d WHERE d.Code = c.Devise);

    ALTER TABLE dbo.CommandeAchat ADD CONSTRAINT FK_CommandeAchat_Devise FOREIGN KEY (Devise) REFERENCES dbo.Devise(Code);
END;

/* Devise et taux d'acquisition du bien */
IF COL_LENGTH('dbo.Immo', 'DeviseAcquisition') IS NULL
    ALTER TABLE dbo.Immo ADD DeviseAcquisition char(3) NULL, TauxAcquisition decimal(18,6) NULL;

/* ---------------------------------------------------------------------------
   Menus et droits : gestion des devises dans Paramètres
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo._Module WHERE IdModule = 10022)
BEGIN
    SET IDENTITY_INSERT dbo._Module ON;
    INSERT INTO dbo._Module (IdModule, Nom) VALUES (10022, 'Devise');
    SET IDENTITY_INSERT dbo._Module OFF;
END;

DECLARE @IdModule bigint = 10022;
DECLARE @IdParametres int = (SELECT IdGroupMenu FROM dbo._GroupMenu WHERE Nom = 'Parametres');
DECLARE @IdInvisible int = 9;

DECLARE @Menus TABLE (Nom nvarchar(250), Libelle nvarchar(200), Titre nvarchar(200), Commentaire nvarchar(400), Url nvarchar(200), Ordre int, IdGroupMenu int, Visible bit, IdAction int);
INSERT INTO @Menus VALUES
 ('GetDevise',        'Devises',            'Devises et taux de change', 'Devises utilisables dans les commandes d''achat et leur taux vers la devise de référence', '/devise/',        1, @IdParametres, 1, 1),
 ('PostDeviseAdd',    'Ajouter une devise',  'Ajouter une devise',        'Création d''une devise',                                                                     '/devise/add/',    1, @IdInvisible,  0, 2),
 ('PostDeviseModify', 'Modifier une devise', 'Modifier une devise',       'Modification du libellé, du taux ou de l''état d''une devise',                               '/devise/modify/', 2, @IdInvisible,  0, 3);

INSERT INTO dbo._Menu (Nom, Libelle, Icone, Commentaire, Titre, Url, Ordre, IdGroupMenu, Visible, IdModule, IdAction, DefaultMenu)
SELECT m.Nom, m.Libelle, NULL, m.Commentaire, m.Titre, m.Url,
       m.Ordre + ISNULL((SELECT MAX(x.Ordre) FROM dbo._Menu x WHERE x.IdGroupMenu = m.IdGroupMenu), 0),
       m.IdGroupMenu, m.Visible, @IdModule, m.IdAction, 0
FROM @Menus m
WHERE NOT EXISTS (SELECT 1 FROM dbo._Menu x WHERE x.Nom = m.Nom);

COMMIT TRAN;
PRINT 'Devises installées.';
