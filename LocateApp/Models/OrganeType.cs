using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Models
{
    public class OrganeType
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Sigle { get; set; }
        public int Ordre { get; set; }
    }
}