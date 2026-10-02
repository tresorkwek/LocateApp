using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace LocateApp.DataTransferObjects
{   

    [ExcludeFromCodeCoverage]
    public class EtiquetteInsertRequest
    {
        public Guid IdCommande { get; set; }
        public int IdFormat { get; set; }
        public string UserCreation { get; set; }
    }

}