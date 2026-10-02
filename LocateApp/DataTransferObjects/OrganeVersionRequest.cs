using System.Diagnostics.CodeAnalysis;

namespace LocateApp.DataTransferObjects
{
    [ExcludeFromCodeCoverage]
    public class InsertOrganeVersionRequest
    {
        public bool Defaut { get; set; }
        public string Libelle { get; set; }
        public string Commentaire { get; set; }
        /// <summary>Version dont les organes sont recopiés dans la nouvelle version (facultatif).</summary>
        public int? CopierDepuis { get; set; }
        public string UserCreation { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ModifyOrganeVersionRequest
    {
        public int Id { get; set; }
        public bool Defaut { get; set; }
        public string Libelle { get; set; }
        public string Commentaire { get; set; }
    }
}
