using LocateApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LocateApp.DataTransferObjects
{

    //public class HopitalConventionneRequest
    //{
    //    public string IdHopital { get; set; }
    //    public string Denomination { get; set; }
    //    public string IdNat { get; set; }
    //    public string Rccm { get; set; }
    //    public string Adresse { get; set; }
    //    public DateTime? DateConv { get; set; }
    //    public string RefDocInterne { get; set; }
    //    public bool Actif { get; set; }

    //    public List<ServicesHopitalRequest> ServicesMedicaux { get; set; } = new List<ServicesHopitalRequest>();
    //}

    //public class HopitalConventionneListRequest
    //{
    //    public string IdHopital { get; set; }
    //    public string Denomination { get; set; }
    //}

    //public class HopitalConventionneRequestInsertion
    //{
    //    public string IdHopital { get; set; }
    //    public string Denomination { get; set; }
    //    public string IdNat { get; set; }
    //    public string Rccm { get; set; }
    //    public string Adresse { get; set; }
    //    public DateTime? DateConv { get; set; }
    //    public string RefDocInterne { get; set; }
    //    public bool Actif { get; set; }
    //    public DateTime DateCreation { get; set; } = DateTime.Now;

    //    [Required]
    //    public string UserCreation { get; set; }

    //    public List<ServicesHopitalRequestModify> ServicesHopital { get; set; } = new List<ServicesHopitalRequestModify>();
    //    //public ServicesHopital[] ServicesHopital { get; set; }
    //}

    //public class HopitalConventionneRequestModify
    //{
    //    public string IdHopital { get; set; }
    //    public string Denomination { get; set; }
    //    public string IdNat { get; set; }
    //    public string Rccm { get; set; }
    //    public string Adresse { get; set; }
    //    public DateTime? DateConv { get; set; }
    //    public string RefDocInterne { get; set; }
    //    public bool Actif { get; set; }
    //    public DateTime? DateCreation { get; set; }
    //    public string UserCreation { get; set; }

    //    public List<ServicesHopitalRequestModify> ServicesHopital { get; set; } = new List<ServicesHopitalRequestModify>();
    //}

    //public class HopitalConventionneForModifyDisplayRequest
    //{
    //    public string IdHopital { get; set; }
    //    public string Denomination { get; set; }
    //    public string IdNat { get; set; }
    //    public string Rccm { get; set; }
    //    public string Adresse { get; set; }
    //    public DateTime? DateConv { get; set; }
    //    public string RefDocInterne { get; set; }
    //    public bool Actif { get; set; }
    //    public DateTime? DateCreation { get; set; }
    //    public string UserCreation { get; set; }

    //    public List<ServicesHopitalForModifyDisplayRequest> ServicesHopital { get; set; } = new List<ServicesHopitalForModifyDisplayRequest>();
    //}

    //public class HopitalConventionneRequestModifyAllFieldsButDateCreation
    //{
    //    public string IdHopital { get; set; }
    //    public string Denomination { get; set; }
    //    public string IdNat { get; set; }
    //    public string Rccm { get; set; }
    //    public string Adresse { get; set; }
    //    public DateTime? DateConv { get; set; }
    //    public string RefDocInterne { get; set; }
    //    public bool Actif { get; set; }
    //    //public DateTime? DateCreation { get; set; }
    //    public string UserCreation { get; set; }

    //    public List<ServicesHopital> ServicesHopital { get; set; } = new List<ServicesHopital>();
    //}

    //public class HopitalConventionneRequestModifySomeFields
    //{
    //    [Required]
    //    public string IdHopital { get; set; }

    //    [Required]
    //    public string Denomination { get; set; }
    //    [Required]
    //    public string IdNat { get; set; }

    //    [Required]
    //    public string Rccm { get; set; }

    //    [Required]
    //    public bool Actif { get; set; }

    //    [Required]
    //    public string UserCreation { get; set; }


    //    public List<ServicesHopital> ServicesHopital { get; set; } = new List<ServicesHopital>();
    //}

    //public class HopitalConventionneRequestDelete
    //{
    //    public string IdHopital { get; set; }
    //}

}