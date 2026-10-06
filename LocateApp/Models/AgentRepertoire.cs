namespace LocateApp.Models
{
    /// <summary>Agent du répertoire (page /agent/) avec son organe, son compte utilisateur éventuel et le nombre de biens dont il est responsable.</summary>
    public class AgentRepertoire : Agent
    {
        public string NomOrgane { get; set; }
        public string IdStructure { get; set; }
        public string NomStructure { get; set; }
        public string Compte { get; set; }
        public string Profil { get; set; }
        public bool CompteActif { get; set; }
        public int NbreBiens { get; set; }

        public string MatriculeCourt => string.IsNullOrEmpty(Matricule) ? "" : Matricule.Trim().Length >= 6 ? Matricule.Trim().Substring(0, 6) : Matricule.Trim();
        public string NomComplet => $"{Nom?.Trim()} {Postnom?.Trim()} {Prenom?.Trim()}".Trim();
    }
}
