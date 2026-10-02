using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace LocateApp.DataTransferObjects
{   

    [ExcludeFromCodeCoverage]
    public class AddFamilleRequest
    {
        public string Nom { get; set; }
        public string Couleur { get; set; }
        public string UserCreation { get; set; }

    }

    [ExcludeFromCodeCoverage]
    public class ModifyFamillieRequest
    {
        public long Id { get; set; }
        public string Nom { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateCreation { get; set; }
        public string UserCreation { get; set; }
        public string Couleur { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class FamillieStatPieRequest
    {
        public long Value { get; set; }
        public string Highlight { get; set; }
        public string Color { get; set; }
        public string Label { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class FamillieStatPieLegendRequest
    {
        public long Data { get; set; }
        public string Color { get; set; }
        public string Label { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class FamillieStatRadardRequest
    {
        public string Label { get; set; }
        public float PourcentageBon { get; set; }
        public float PourcentageMauvais { get; set; }
    }

}