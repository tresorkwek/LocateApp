using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace LocateApp.DataTransferObjects
{


    [ExcludeFromCodeCoverage]
    public class InsertOrganeTypeRequest
    {
        public string Nom { get; set; }
        public string Sigle { get; set; }
        public int Ordre { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ModifyOrganeTypeRequest
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Sigle { get; set; }
        public int Ordre { get; set; }
    }
}