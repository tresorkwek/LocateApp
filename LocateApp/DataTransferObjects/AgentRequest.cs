using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Web;

namespace LocateApp.DataTransferObjects
{
    [ExcludeFromCodeCoverage]
    public class ResponsabletRequest
    {
        public string Matricule { get; set; }
        public long Nbre { get; set; }

    }


}