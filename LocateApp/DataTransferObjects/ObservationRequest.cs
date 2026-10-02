using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace LocateApp.DataTransferObjects
{   

    [ExcludeFromCodeCoverage]
    public class AddObservationRequest
    {
        public string Observation { get; set; }
        public string Etat { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ModifyObservationRequest
    {
        public int Id { get; set; }
        public string Observation { get; set; }
        public string Etat { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class SelectStatObservationRequest
    {
        public string Observation { get; set; }
        public long NbreBon { get; set; }
        public float PourcentageBon { get; set; }
        public long NbreMauvais { get; set; }
        public float PourcentageMauvais { get; set; }
    }
}