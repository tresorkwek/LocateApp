using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace LocateApp.DataTransferObjects
{   

    [ExcludeFromCodeCoverage]
    public class CommandeInsertRequest
    {
        public int NbrePage { get; set; }
        public int IdFormat { get; set; }
        public string CreatedBy { get; set; }

    }

    [ExcludeFromCodeCoverage]
    public class CommandeUpdateRequest
    {
        public Guid Id { get; set; }
        public int NbrePage { get; set; }
        public int IdFormat { get; set; }
        public string CreatedBy { get; set; }

    }

}