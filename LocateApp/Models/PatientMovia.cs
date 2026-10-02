using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Models
{
    public class PatientMovia:AyantDroit
    {
        public int AccordAgent { get; set; }
        public string Commentaire { get; set; }
        public string UserAccordDRH { get; set; }
        public string NomUserAccordDRH { get; set; }
        public string PhotoUserAccordDRH { get; set; }
        public string DateAccordDRH { get; set; }
        public string UserAdminAccord { get; set; }
        public string NomUserAdminAccord { get; set; }
        public string PhotoUserAdminAccord { get; set; }
        public string DateAdminAccord { get; set; }
        public string UserImpression { get; set; }
        public string DateImpression { get; set; }
        public int Retirer { get; set; }
        public string DateRetrait { get; set; } 
    }
}