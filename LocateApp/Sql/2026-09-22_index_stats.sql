-- Locate : index couvrants pour les statistiques du tableau de bord (comptages par année / état, graphiques par famille et observation).
-- Idempotent : peut être rejoué sans risque.
SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_InventaireDetails_Annee_Stats' AND object_id = OBJECT_ID('InventaireDetails'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_InventaireDetails_Annee_Stats
        ON InventaireDetails (Annee)
        INCLUDE (ImmoExist, Etat);
    PRINT 'Index IX_InventaireDetails_Annee_Stats créé.';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Immo_Stats' AND object_id = OBJECT_ID('Immo'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Immo_Stats
        ON Immo (IsActive, LastEtat)
        INCLUDE (QrCode, IdLastObservation, IdArticle, ImmoExist);
    PRINT 'Index IX_Immo_Stats créé.';
END
