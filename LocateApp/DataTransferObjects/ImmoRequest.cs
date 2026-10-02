using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Web;
using LocateApp.Controllers;
using LocateApp.Models;

namespace LocateApp.DataTransferObjects
{   

    [ExcludeFromCodeCoverage]
    public class SearchImmoRequest
    {
        public string Code { get; set; }

    }

    [ExcludeFromCodeCoverage]
    public class AffectQRCodeToImmoRequest
    {
        public long Id { get; set; }
        public Guid? QrCode { get; set; }
        public long? IdEtiquette { get; set; }

    }

    [ExcludeFromCodeCoverage]
    public class ChangeLocalOfImmoRequest
    {
        public long IdImmo { get; set; }
        public Guid QrCodeLocal { get; set; }

    }

    [ExcludeFromCodeCoverage]
    public class ExpedierImmoRequest
    {
        public long IdImmo { get; set; }
        public string CodeOrgane { get; set; }

    }

    [ExcludeFromCodeCoverage]
    public class ChangeLocalDeclassementImmoRequest
    {
        public long Id { get; set; }
        public long IdLocal { get; set; }
        public string DesignationLocal { get; set; } 
        public string UserDeclassement { get; set; }
        public int Annee { get; set; }

    }

    [ExcludeFromCodeCoverage]
    public class DeclassementLocalImmoRequest
    {
        public long IdLocal { get; set; }
        public string UserDeclassement { get; set; }

    }

    [ExcludeFromCodeCoverage]
    public class ChangeLocalImmoRequest
    {
        public long Id { get; set; }
        public long IdLocal { get; set; }
        public string DesignationLocal { get; set; }

    }

    [ExcludeFromCodeCoverage]
    public class MisEnServiceImmoRequest
    {
        public long Id { get; set; }
        public long IdLocal { get; set; }
        public string UserMisEnService { get; set; }

    }

    [ExcludeFromCodeCoverage]
    public class DeclassementImmoRequest
    {
        public long IdImmo { get; set; }
        public string UserDeclassement { get; set; }

    }

    [ExcludeFromCodeCoverage]
    public class DesaffectQRCodeToImmoRequest
    {
        public long Id { get; set; }
        public bool Liberate { get; set; }

    }

    [ExcludeFromCodeCoverage]
    public class InsertImmoRequest
    {
        public string CodeImmo { get; set; }
        public long IdArticle { get; set; }
        public long IdLocal { get; set; }
        public string LastEtat { get; set; }
        public int IdLastObservation { get; set; }
        public int LastAnneeComptable { get; set; }
        public string UserCreation { get; set; }
        public string Responsable { get; set; }
        public string Constat { get; set; }
        public bool Inventaire { get; set; }
        public long? IdImmoParent { get; set; }
        public long? IdImmoPrincipal { get; set; }
        public int Nbre { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class InsertImmoSyncRequest 
    {
        public string CodeImmo { get; set; }
        public long IdArticle { get; set; }
        public long IdLocal { get; set; }
        public string LastEtat { get; set; }
        public int IdLastObservation { get; set; }
        public int LastAnneeComptable { get; set; }
        public string UserCreation { get; set; }
        public string Responsable { get; set; }
        public string Constat { get; set; }
        public long? IdImmoParent { get; set; }
        public long? IdImmoPrincipal { get; set; }
        public Guid? QrCode { get; set; }
        public long? IdEtiquette { get; set; }
        public string Inventorieur { get; set; }
    }


    [ExcludeFromCodeCoverage]
    public class ModifyImmoRequest
    {
        public long Id { get; set; }
        public string CodeImmo { get; set; }
        public long IdArticle { get; set; }
        public long IdLocal { get; set; }
        public bool IsActive { get; set; }
        public string Responsable { get; set; }
        public string LastEtat { get; set; }
        public int IdLastObservation { get; set; }
        public string UserCreation { get; set; }
        public DateTime DateCreation { get; set; }
        public string Constat { get; set; }
        public string IdPhoto { get; set; }
        public string ConstatToModify { get; set; }
        public string IdPhotoToDelete { get; set; }
        public long? IdImmoParent { get; set; }
        public long IdImmoPrincipal { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ModifyImmoSyncRequest : ModifyImmoRequest
    {
        public Guid? QrCode { get; set; }
        public long? IdEtiquette { get; set; }
        public string Inventorieur { get; set; }
        public bool ImmoExist { get; set; }
        public string CodeOrgane { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class InsertPhotoImmoFoTabletRequest
    {
        public string IdImmo { get; set; }
        public string Constat { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class InsertPhotoImmoRequest
    {
        public Guid Id { get; set; }
        public long IdImmo { get; set; }
        public string Constat { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class GetImmoRequest : Immo
    {
        public GetArticleRequest Article => IdArticle == null ? null : ArticleController.SelectRequesById((long)IdArticle).FirstOrDefault();
        public GetLocalRequest Local => IdLocal == null ? null : LocalController.SelectRequestById((long)IdLocal).FirstOrDefault();
        public List<ImmoPhotos> ImmoPhotos => GetImmoPhotos();
    }
}