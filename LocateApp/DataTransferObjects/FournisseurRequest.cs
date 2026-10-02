using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace LocateApp.DataTransferObjects
{   

    [ExcludeFromCodeCoverage]
    public class AddFournisseurRequest
    {
        public string Nom { get; set; }
        public string Adresse { get; set; }
        public string UserCreation { get; set; }

    }

    [ExcludeFromCodeCoverage]
    public class ModifyFournisseurRequest
    {
        public long Id { get; set; }
        public string Nom { get; set; }
        public string Adresse { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateCreation { get; set; }
        public string UserCreation { get; set; }
    }

}