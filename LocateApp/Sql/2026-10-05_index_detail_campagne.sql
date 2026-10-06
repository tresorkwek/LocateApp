/* ============================================================================
   Locate - Index couvrant du détail d'une campagne d'inventaire. Idempotent.

   La page /inventaire/details/{annee} (bilan par organe, puis lignes d'un organe)
   lit InventaireDetails par Annee + CodeOrgane. L'index (Annee, CodeOrgane) de
   2026-10-01_index_inventaire_details.sql obligeait à relire chaque ligne dans la
   table (clé primaire en GUID, lectures dispersées) : sur une machine où SQL Server
   manque de mémoire, l'organe le plus chargé (1 749 lignes) dépassait le délai de 30 s.
   L'index inclut désormais les colonnes lues par ces deux requêtes.
   ============================================================================ */
SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1
               FROM sys.indexes i
                    JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 1
                    JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
               WHERE i.object_id = OBJECT_ID('dbo.InventaireDetails') AND i.name = 'IX_InventaireDetails_Annee_CodeOrgane' AND c.name = 'DesignationLocal')
BEGIN
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.InventaireDetails') AND name = 'IX_InventaireDetails_Annee_CodeOrgane')
        CREATE NONCLUSTERED INDEX IX_InventaireDetails_Annee_CodeOrgane ON dbo.InventaireDetails (Annee, CodeOrgane)
            INCLUDE (IdImmo, ImmoExist, Etat, IdObservation, DateCreation, UserCreation, Responsable, IdLocal, Observation, DesignationLocal)
            WITH (DROP_EXISTING = ON);
    ELSE
        CREATE NONCLUSTERED INDEX IX_InventaireDetails_Annee_CodeOrgane ON dbo.InventaireDetails (Annee, CodeOrgane)
            INCLUDE (IdImmo, ImmoExist, Etat, IdObservation, DateCreation, UserCreation, Responsable, IdLocal, Observation, DesignationLocal);
END

PRINT 'Index du détail des campagnes d''inventaire installé.';
