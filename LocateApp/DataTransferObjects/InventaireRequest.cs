using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace LocateApp.DataTransferObjects
{   

    [ExcludeFromCodeCoverage]
    public class InsertInventaireDetailsRequest
    {
        public int Annee { get; set; }
        public long IdImmo { get; set; }
        public bool ImmoExist { get; set; }
        public string Etat { get; set; }
        public int IdObservation { get; set; }
        public string UserCreation { get; set; }
        public string Responsable { get; set; }
        public long IdLocal { get; set; }
        public string CodeOrgane { get; set; }
        public string DesignationLocal { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class InsertInventaireLocalRequest
    {
        public int Annee { get; set; }
        public string UserCreation { get; set; }
        public long IdLocal { get; set; }
        public string CodeOrgane { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class LancementInventaireRequest
    {
        public int Annee { get; set; }
        public string UserCreation { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ModifyInventaireRequest
    {
        public int Annee { get; set; }
        public bool Actif { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class CloturenventaireRequest
    {
        public int Annee { get; set; }
        public string UserCloture { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class SelectQuantiteImmoByArticleRequest
    {
        public int Annee { get; set; }
        public long IdArticle { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class SelectQuantiteImmoByArticleAndLocalRequest
    {
        public int Annee { get; set; }
        public long IdArticle { get; set; }
        public long IdLocal { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class SelectQuantiteImmoByLocalRequest
    {
        public int Annee { get; set; }
        public long IdLocal { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class SelectQuantiteImmoByOrganeRequest
    {
        public int Annee { get; set; }
        public string CodeOrgane { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class SelectQuantiteImmoByResponsableRequest
    {
        public int Annee { get; set; }
        public string Responsable { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class DeclareImmoNonVuRequest
    {
        public long IdImmo { get; set; }
        public string Observation { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class InsertInventaireDetailsNonVuRequest
    {
        public int Annee { get; set; }
        public long IdImmo { get; set; }
        public bool ImmoExist { get; set; }
        public string Etat { get; set; }
        public int? IdObservation { get; set; }
        public string UserCreation { get; set; }
        public string Responsable { get; set; }
        public long IdLocal { get; set; }
        public string CodeOrgane { get; set; }
        public string Observation { get; set; }
        public string DesignationLocal { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class IdentifierImmoRequest
    {
        public long IdImmo { get; set; }
        public string LastEtat { get; set; }
        public int IdLastObservation { get; set; }
        public string UserVu { get; set; }

    }

}