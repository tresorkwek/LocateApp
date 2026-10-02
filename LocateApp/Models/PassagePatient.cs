using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Models
{
    public class PassagePatient
    {
        public string Matricule { get; set; }
        public DateTime DatePassage { get; set; }
        public string IdHopital { get; set; }
        public long IdServiceHopital { get; set; }
        public long IdBilletEnvoi { get; set; }
        public string UserCreation { get; set; }
        public bool IsUrgent { get; set; }
        public string Observation { get; set; }
    }
}