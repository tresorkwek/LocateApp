using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Models
{
    public class ServiceMedical
    {
        public string IdService { get; set; }
        public string Denomination { get; set; }
        public string Acronyme { get; set; }
        public bool AutoApprobation { get; set; }
    }
}