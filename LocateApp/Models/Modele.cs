using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocateApp.Controllers;

namespace LocateApp.Models
{
    public class Modele
    {
        public int Id { get; set; }
        public string Designation { get; set; }
        public int IdMarque { get; set; }
        public bool IsActive { get; set; }

    }
}