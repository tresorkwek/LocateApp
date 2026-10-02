namespace LocateApp.Repository
{
    public static class SqlOrganeVersion
    {
        private const string Colonnes = @"Id, Defaut, Libelle, Commentaire, DateCreation, UserCreation,
                                          (SELECT COUNT(*) FROM Organe o WHERE o.Version = OrganeVersion.Id) AS NbOrgane";

        public static string SelectAll { get; } = "SELECT " + Colonnes + " FROM OrganeVersion ORDER BY Id";

        public static string SelectById { get; } = "SELECT " + Colonnes + " FROM OrganeVersion WHERE Id = @Id";

        public static string SelectDefault { get; } = "SELECT " + Colonnes + " FROM OrganeVersion WHERE Defaut = 1";

        public static string Insert { get; } = @"INSERT INTO OrganeVersion (Defaut, Libelle, Commentaire, DateCreation, UserCreation)
                                                VALUES (@Defaut, @Libelle, @Commentaire, GETDATE(), @UserCreation);
                                                SELECT CAST(SCOPE_IDENTITY() AS int)";

        public static string Update { get; } = @"UPDATE OrganeVersion SET Libelle = @Libelle, Commentaire = @Commentaire WHERE Id = @Id";

        /* Bascule de la version courante : l'application exige exactement une version Defaut = 1 */
        public static string ResetAllDefault { get; } = @"UPDATE OrganeVersion SET Defaut = 0";

        public static string SetDefault { get; } = @"UPDATE OrganeVersion SET Defaut = 1 WHERE Id = @Id";

        public static string Delete { get; } = @"DELETE FROM OrganeVersion
                                                WHERE Id = @Id AND Defaut = 0 AND NOT EXISTS (SELECT 1 FROM Organe WHERE Version = @Id)";

        /* Copie des organes d'une version vers une autre : le code garde sa longueur, seuls les deux premiers
           chiffres (numéro de version) changent, pour l'organe, son parent et sa structure. */
        public static string CopierOrganes { get; } = @"INSERT INTO Organe (Id, Nom, Sigle, IndCoresp, Fictif, Actif, IdTypeOrgane, IdOrganeParent, Interne, Ordre, IdStructure, OrdreInterne, Adresse, Version, Entite)
                                                       SELECT @Prefix + SUBSTRING(s.Id, 3, LEN(s.Id)), s.Nom, s.Sigle, s.IndCoresp, s.Fictif, s.Actif, s.IdTypeOrgane,
                                                              CASE WHEN s.IdOrganeParent IS NULL THEN NULL ELSE @Prefix + SUBSTRING(s.IdOrganeParent, 3, LEN(s.IdOrganeParent)) END,
                                                              s.Interne, s.Ordre, @Prefix + SUBSTRING(s.IdStructure, 3, LEN(s.IdStructure)), s.OrdreInterne, s.Adresse, @Version, s.Entite
                                                       FROM Organe s
                                                       WHERE s.Version = @Source
                                                         AND NOT EXISTS (SELECT 1 FROM Organe t WHERE t.Id = @Prefix + SUBSTRING(s.Id, 3, LEN(s.Id)))";
    }
}
