using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;
using LocateApp.Models;
using LocateApp.Controllers;

namespace LocateApp.DataTransferObjects
{   

    [ExcludeFromCodeCoverage]
    public class AffectQRCodeToLocalRequest
    {
        public long Id { get; set; }
        public Guid QrCode { get; set; }
        public long? IdEtiquette { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class AddLocalRequest
    {
        public string Code { get; set; }
        public string Designation { get; set; }
        public string CodeOrgane { get; set; }
        public bool IsSpace { get; set; }
        public string UserCreation { get; set; }
        public int? IdTypeLocal { get; set; }

    }

    [ExcludeFromCodeCoverage]
    public class AddLocalSyncRequest
    {
        public string Code { get; set; }
        public string Designation { get; set; }
        public string CodeOrgane { get; set; }
        public bool IsSpace { get; set; }
        public string UserCreation { get; set; }
        public Guid? QrCode { get; set; }
        public long? IdEtiquette { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ModifyLocalRequest
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Designation { get; set; }
        public string CodeOrgane { get; set; }
        public bool IsSpace { get; set; }
        public bool IsActive { get; set; }
        public int? IdTypeLocal { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class GetLocalRequest : Local
    {
        public Organe Organe => GetOrgane();

    }

}