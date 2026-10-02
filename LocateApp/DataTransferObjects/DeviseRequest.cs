using System.Diagnostics.CodeAnalysis;

namespace LocateApp.DataTransferObjects
{
    [ExcludeFromCodeCoverage]
    public class AddDeviseRequest
    {
        public string Code { get; set; }
        public string Libelle { get; set; }
        public string Symbole { get; set; }
        public decimal Taux { get; set; }
        public bool EstReference { get; set; }
        public bool Actif { get; set; }
        public string UserMaj { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ModifyDeviseRequest : AddDeviseRequest
    {
    }
}
