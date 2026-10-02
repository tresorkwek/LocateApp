using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace LocateApp.DataTransferObjects
{  

    [ExcludeFromCodeCoverage]
    public class UpSetServiceMedicalRequest
    {
        public string IdService { get; set; }
        public string Denomination { get; set; }
        public string Acronyme { get; set; }
        public bool AutoApprobation { get; set; }
    }
}