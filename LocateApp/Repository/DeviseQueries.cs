namespace LocateApp.Repository
{
    public static class SqlDevise
    {
        private const string Colonnes = "Code, Libelle, Symbole, Taux, EstReference, Actif, DateMaj, UserMaj";

        public static string SelectAll { get; } = "SELECT " + Colonnes + " FROM Devise ORDER BY EstReference DESC, Code";

        public static string SelectActives { get; } = "SELECT " + Colonnes + " FROM Devise WHERE Actif = 1 ORDER BY EstReference DESC, Code";

        public static string SelectByCode { get; } = "SELECT " + Colonnes + " FROM Devise WHERE Code = @Code";

        public static string SelectReference { get; } = "SELECT TOP 1 " + Colonnes + " FROM Devise ORDER BY EstReference DESC, Code";

        public static string Insert { get; } = @"INSERT INTO Devise (Code, Libelle, Symbole, Taux, EstReference, Actif, UserMaj)
                                                VALUES (@Code, @Libelle, @Symbole, @Taux, @EstReference, @Actif, @UserMaj)";

        public static string Update { get; } = @"UPDATE Devise
                                                SET Libelle = @Libelle, Symbole = @Symbole, Taux = @Taux, EstReference = @EstReference, Actif = @Actif,
                                                    DateMaj = GETDATE(), UserMaj = @UserMaj
                                                WHERE Code = @Code";

        /* Une seule devise de référence : les autres repassent à 0 et la référence garde un taux de 1 */
        public static string ResetReference { get; } = @"UPDATE Devise SET EstReference = 0 WHERE Code <> @Code";

        public static string ForceTauxReference { get; } = @"UPDATE Devise SET Taux = 1, Actif = 1 WHERE Code = @Code";
    }
}
