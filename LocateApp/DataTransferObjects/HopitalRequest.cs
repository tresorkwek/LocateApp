using LocateApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace LocateApp.DataTransferObjects
{
    [ExcludeFromCodeCoverage]
    public class SelectOrDeleteHopitalRequest
    {
        public string ValueToSelect { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class UpSetHopitalRequest
    {
        public string Id { get; set; }
        public string Denomination { get; set; }
        public string Acronyme { get; set; }
        public string IdNat { get; set; }
        public string Rccm { get; set; }
        public DateTime DateConv { get; set; }
        public string RefDocInterne { get; set; }
        public bool IsActive { get; set; }
        public bool IsPublic { get; set; }
        public bool IsHospital { get; set; }       
        public string Services { get; set; }
        public string Adresse { get; set; }
    }

}