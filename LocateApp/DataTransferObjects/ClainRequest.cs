using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace LocateApp.DataTransferObjects
{   

    [ExcludeFromCodeCoverage]
    public class AddUpdateClaimRequest
    {
        public int IdProfil { get; set; }
        public string IdMenu { get; set; }
        public string CustomUrl { get; set; }
        public bool Update { get; set; }
    }
   
}