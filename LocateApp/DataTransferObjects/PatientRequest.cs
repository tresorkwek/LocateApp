using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics.CodeAnalysis;

namespace LocateApp.DataTransferObjects
{
    [ExcludeFromCodeCoverage]
    public class MatriculePatientRequest
    {
        public string Matricule { get; set; }
    }
    [ExcludeFromCodeCoverage]
    public class ImprimerPatientRequest 
    {
        public string UserName { get; set; }
        public string Matricule { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class NamePatientRequest
    {
        public string Nom { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ModifyInfoPatientRequest
    {
        public string Matricule { get; set; }
        public string Nom { get; set; }
        public string Postnom { get; set; }
        public string Prenom { get; set; }
        public int Sexe { get; set; }
        public string Commentaire { get; set; }
    }
        
    [ExcludeFromCodeCoverage]
    public class GetModificationPatientRequest
    {
        public string Matricule { get; set; }
        public string Title { get; set; }
        public int TypeTitle { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class CorrectInfoPatientRequest
    {
        public string Matricule { get; set; }
        public string UserAccordDRH { get; set; }

    }
}