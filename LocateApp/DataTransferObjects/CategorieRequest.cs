using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace LocateApp.DataTransferObjects
{   

    [ExcludeFromCodeCoverage]
    public class AddCategorieRequest
    {
        public string Designation { get; set; }
        public long IdFamille { get; set; }
        public string UserCreation { get; set; }

    }

    [ExcludeFromCodeCoverage]
    public class ModifyCategorieRequest
    {
        public long Id { get; set; }
        public string Designation { get; set; }
        public bool IsActive { get; set; }
        public long IdFamille { get; set; }
        public DateTime DateCreation { get; set; }
        public string UserCreation { get; set; }
    }

}