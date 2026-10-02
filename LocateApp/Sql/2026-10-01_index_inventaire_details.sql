/* ============================================================================
   Locate - Index des compteurs d'inventaire. Idempotent.

   Les compteurs « inventoriés » d'un local et d'un organe (affichés pour chaque
   local de /local/organe/{code} et dans le mobile) comptent les lignes de
   InventaireDetails par Annee + IdLocal et Annee + CodeOrgane. Sans index, chaque
   compteur parcourt toute la table : sur une machine chargée, un organe de 6
   locaux mettait plus de 90 s à répondre et le mobile abandonnait.
   ============================================================================ */
SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.InventaireDetails') AND name = 'IX_InventaireDetails_Annee_IdLocal')
    CREATE NONCLUSTERED INDEX IX_InventaireDetails_Annee_IdLocal ON dbo.InventaireDetails (Annee, IdLocal);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.InventaireDetails') AND name = 'IX_InventaireDetails_Annee_CodeOrgane')
    CREATE NONCLUSTERED INDEX IX_InventaireDetails_Annee_CodeOrgane ON dbo.InventaireDetails (Annee, CodeOrgane);

PRINT 'Index des compteurs d''inventaire installés.';
